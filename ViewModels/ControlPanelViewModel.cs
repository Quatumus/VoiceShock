using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenShock.SDK.CSharp;
using OpenShock.SDK.CSharp.Models;
using Silk.NET.SDL;
using Whisper.net;
using Whisper.net.Ggml;
using VoiceShock.Data;
using VoiceShock.Models;

namespace VoiceShock.ViewModels;

public partial class ControlPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _placeholderText = "Control Panel";

    [ObservableProperty]
    private string _transcriptionText = string.Empty;

    [ObservableProperty]
    private bool _isTranscribing;

    [ObservableProperty]
    private ObservableCollection<MicrophoneDevice> _devices = new();

    [ObservableProperty]
    private MicrophoneDevice? _selectedDevice;

    [ObservableProperty]
    private ObservableCollection<string> _languages = new();

    [ObservableProperty]
    private string? _selectedLanguage;

    [ObservableProperty]
    private bool _showLanguageSelection;

    [ObservableProperty]
    private ObservableCollection<string> _gpuOptions = new();

    [ObservableProperty]
    private string? _selectedGpuOption;

    [ObservableProperty]
    private ObservableCollection<GpuDevice> _gpuDevices = new();

    [ObservableProperty]
    private GpuDevice? _selectedGpuDevice;

    [ObservableProperty]
    private bool _showGpuDeviceSelection;

    partial void OnSelectedDeviceChanged(MicrophoneDevice? value)
    {
        if (value == null) return;
        
        using var db = new Data.AppDbContext();
        var setting = db.Settings.FirstOrDefault(s => s.Key == "SelectedMicrophone");
        if (setting == null)
        {
            db.Settings.Add(new Setting { Key = "SelectedMicrophone", Value = value.Name });
        }
        else
        {
            setting.Value = value.Name;
        }
        db.SaveChanges();
    }

    partial void OnSelectedLanguageChanged(string? value)
    {
        if (value == null) return;
        
        using var db = new Data.AppDbContext();
        var setting = db.Settings.FirstOrDefault(s => s.Key == "SelectedLanguage");
        if (setting == null)
        {
            db.Settings.Add(new Setting { Key = "SelectedLanguage", Value = value });
        }
        else
        {
            setting.Value = value;
        }
        db.SaveChanges();
    }

    partial void OnSelectedModelChanged(WhisperModelOption? value)
    {
        if (value == null) return;
        
        // Show language selection only for multilingual models
        ShowLanguageSelection = !value.Type.ToString().Contains("En");
        
        // Save selected model to database
        using var db = new Data.AppDbContext();
        var setting = db.Settings.FirstOrDefault(s => s.Key == "SelectedModel");
        if (setting == null)
        {
            db.Settings.Add(new Setting { Key = "SelectedModel", Value = value.Name });
        }
        else
        {
            setting.Value = value.Name;
        }
        db.SaveChanges();
    }

    partial void OnSelectedGpuOptionChanged(string? value)
    {
        if (value == null) return;
        
        // Show GPU device selection only when GPU acceleration is chosen
        ShowGpuDeviceSelection = value == "Vulkan (GPU)";
        
        using var db = new Data.AppDbContext();
        var setting = db.Settings.FirstOrDefault(s => s.Key == "SelectedGpuOption");
        if (setting == null)
        {
            db.Settings.Add(new Setting { Key = "SelectedGpuOption", Value = value });
        }
        else
        {
            setting.Value = value;
        }
        db.SaveChanges();
    }

    partial void OnSelectedGpuDeviceChanged(GpuDevice? value)
    {
        if (value == null) return;
        
        using var db = new Data.AppDbContext();
        var setting = db.Settings.FirstOrDefault(s => s.Key == "SelectedGpuDevice");
        if (setting == null)
        {
            db.Settings.Add(new Setting { Key = "SelectedGpuDevice", Value = value.Name });
        }
        else
        {
            setting.Value = value.Name;
        }
        db.SaveChanges();
    }

    private Sdl? _sdl;
    private uint _audioDevice;
    private AudioCallback _audioCallbackDelegate;
    
    private WhisperProcessor? _processor;
    private WhisperFactory? _factory;
    private MemoryStream? _waveStream;

    [ObservableProperty]
    private ObservableCollection<WhisperModelOption> _models = new();

    [ObservableProperty]
    private WhisperModelOption? _selectedModel;

    public unsafe ControlPanelViewModel()
    {
        _sdl = Sdl.GetApi();
        _sdl.Init(Sdl.InitAudio);
        _audioCallbackDelegate = AudioCallbackInternal;
        LoadDevices();
        
        // Load saved microphone selection after devices are loaded
        LoadSavedMicrophoneSelection();
        
        // Initialize language options
        InitializeLanguages();
        LoadSavedLanguageSelection();
        
        // Initialize GPU options
        InitializeGpuOptions();
        LoadSavedGpuSelection();
        
        // Initialize GPU devices
        InitializeGpuDevices();
        LoadSavedGpuDeviceSelection();

        BuildModels();
        RefreshInstalledModels();
        
        // Load saved model selection
        LoadSavedModelSelection();
        
        SelectedModel = Models.FirstOrDefault(m => m.IsInstalled) ?? Models.FirstOrDefault();
    }

    private void InitializeLanguages()
    {
        Languages = new ObservableCollection<string>(new[]
        {
            "auto", "en", "es", "fr", "de", "it", "pt", "ru", "ja", "ko", "zh", "ar", "hi", "th", "vi", "nl", "sv", "da", "no", "fi"
        });
    }

    private void InitializeGpuOptions()
    {
        GpuOptions = new ObservableCollection<string>(new[]
        {
            "CPU", "Vulkan (GPU)"
        });
    }

    private void LoadSavedGpuSelection()
    {
        using (var db = new Data.AppDbContext())
        {
            var savedGpu = db.Settings.FirstOrDefault(s => s.Key == "SelectedGpuOption");
            if (savedGpu != null)
            {
                var gpuOption = GpuOptions.FirstOrDefault(g => g == savedGpu.Value);
                if (gpuOption != null)
                {
                    SelectedGpuOption = gpuOption;
                    System.Console.WriteLine($"[DEBUG_LOG] Loaded saved GPU option: {gpuOption}");
                    return;
                }
                else
                {
                    System.Console.WriteLine($"[DEBUG_LOG] Saved GPU option '{savedGpu.Value}' not found in available options");
                }
            }
            else
            {
                System.Console.WriteLine("[DEBUG_LOG] No saved GPU option found in database");
            }
        }
        
        // Fallback: select CPU
        SelectedGpuOption = GpuOptions.FirstOrDefault();
        System.Console.WriteLine($"[DEBUG_LOG] Using default GPU option: {SelectedGpuOption}");
    }

    private void InitializeGpuDevices()
    {
        GpuDevices.Clear();
        
        // Add CPU option
        GpuDevices.Add(new GpuDevice 
        { 
            Name = "CPU", 
            Type = "CPU", 
            Vendor = "Generic", 
            MemoryMB = 0, 
            IsAvailable = true 
        });
        
        // Try to detect Vulkan devices
        try
        {
            // This is a simplified detection - in a real implementation you'd use Vulkan API
            // For now, we'll add common GPU options that users can select
            GpuDevices.Add(new GpuDevice 
            { 
                Name = "Integrated Graphics", 
                Type = "Integrated", 
                Vendor = "AMD/Intel", 
                MemoryMB = 1024, // Estimate
                IsAvailable = true 
            });
            
            GpuDevices.Add(new GpuDevice 
            { 
                Name = "Discrete Graphics", 
                Type = "Discrete", 
                Vendor = "AMD/NVIDIA", 
                MemoryMB = 4096, // Estimate
                IsAvailable = true 
            });
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Error detecting GPU devices: {ex.Message}");
        }
    }

    private void LoadSavedGpuDeviceSelection()
    {
        using (var db = new Data.AppDbContext())
        {
            var savedGpuDevice = db.Settings.FirstOrDefault(s => s.Key == "SelectedGpuDevice");
            if (savedGpuDevice != null)
            {
                var device = GpuDevices.FirstOrDefault(g => g.Name == savedGpuDevice.Value);
                if (device != null)
                {
                    SelectedGpuDevice = device;
                    System.Console.WriteLine($"[DEBUG_LOG] Loaded saved GPU device: {device.Name}");
                    return;
                }
                else
                {
                    System.Console.WriteLine($"[DEBUG_LOG] Saved GPU device '{savedGpuDevice.Value}' not found in available devices");
                }
            }
            else
            {
                System.Console.WriteLine("[DEBUG_LOG] No saved GPU device found in database");
            }
        }
        
        // Fallback: select CPU
        SelectedGpuDevice = GpuDevices.FirstOrDefault();
        System.Console.WriteLine($"[DEBUG_LOG] Using default GPU device: {SelectedGpuDevice?.Name}");
    }

    private void LoadSavedLanguageSelection()
    {
        using (var db = new Data.AppDbContext())
        {
            var savedLang = db.Settings.FirstOrDefault(s => s.Key == "SelectedLanguage");
            if (savedLang != null)
            {
                var language = Languages.FirstOrDefault(l => l == savedLang.Value);
                if (language != null)
                {
                    SelectedLanguage = language;
                    System.Console.WriteLine($"[DEBUG_LOG] Loaded saved language: {language}");
                    return;
                }
                else
                {
                    System.Console.WriteLine($"[DEBUG_LOG] Saved language '{savedLang.Value}' not found in available languages");
                }
            }
            else
            {
                System.Console.WriteLine("[DEBUG_LOG] No saved language found in database");
            }
        }
        
        // Fallback: select auto-detection
        SelectedLanguage = Languages.FirstOrDefault();
        System.Console.WriteLine($"[DEBUG_LOG] Using default language: {SelectedLanguage}");
    }

    private void LoadSavedModelSelection()
    {
        using (var db = new Data.AppDbContext())
        {
            var savedModel = db.Settings.FirstOrDefault(s => s.Key == "SelectedModel");
            if (savedModel != null)
            {
                var model = Models.FirstOrDefault(m => m.Name == savedModel.Value);
                if (model != null)
                {
                    SelectedModel = model;
                    System.Console.WriteLine($"[DEBUG_LOG] Loaded saved model: {model.Name}");
                    return;
                }
                else
                {
                    System.Console.WriteLine($"[DEBUG_LOG] Saved model '{savedModel.Value}' not found in available models");
                }
            }
            else
            {
                System.Console.WriteLine("[DEBUG_LOG] No saved model found in database");
            }
        }
    }

    private void LoadSavedMicrophoneSelection()
    {
        using (var db = new Data.AppDbContext())
        {
            var savedMic = db.Settings.FirstOrDefault(s => s.Key == "SelectedMicrophone");
            if (savedMic != null)
            {
                var device = Devices.FirstOrDefault(d => d.Name == savedMic.Value);
                if (device != null)
                {
                    SelectedDevice = device;
                    System.Console.WriteLine($"[DEBUG_LOG] Loaded saved microphone: {device.Name}");
                    return;
                }
                else
                {
                    System.Console.WriteLine($"[DEBUG_LOG] Saved microphone '{savedMic.Value}' not found in available devices");
                }
            }
            else
            {
                System.Console.WriteLine("[DEBUG_LOG] No saved microphone found in database");
            }
        }
        
        // Fallback: select first available device if no saved selection or saved device not found
        if (Devices.Any())
        {
            SelectedDevice = Devices.FirstOrDefault();
            System.Console.WriteLine($"[DEBUG_LOG] Using default microphone: {SelectedDevice?.Name}");
        }
    }

    private void BuildModels()
    {
        Models = new ObservableCollection<WhisperModelOption>(new[]
        {
            new WhisperModelOption(GgmlType.Base, "Base (Multilingual)", "ggml-base.bin"),
            new WhisperModelOption(GgmlType.BaseEn, "Base (English Only)", "ggml-base-en.bin"),
            new WhisperModelOption(GgmlType.Small, "Small (Multilingual)", "ggml-small.bin"),
            new WhisperModelOption(GgmlType.SmallEn, "Small (English Only)", "ggml-small-en.bin"),
        });
    }

    private void RefreshInstalledModels()
    {
        foreach (var m in Models)
        {
            m.IsInstalled = File.Exists(m.FileName);
        }
        // reassign to trigger UI updates if needed
        Models = new ObservableCollection<WhisperModelOption>(Models);
    }

    [RelayCommand]
    private void RefreshModels()
    {
        RefreshInstalledModels();
    }

    [RelayCommand]
    private void DeleteSelectedModel()
    {
        if (SelectedModel == null) return;
        try
        {
            if (File.Exists(SelectedModel.FileName))
            {
                File.Delete(SelectedModel.FileName);
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Failed to delete model {SelectedModel.FileName}: {ex.Message}");
        }
        finally
        {
            RefreshInstalledModels();
        }
    }

    private unsafe void LoadDevices()
    {
        Devices.Clear();
        if (_sdl == null) return;

        int count = _sdl.GetNumAudioDevices(1); // 1 for capture
        for (int i = 0; i < count; i++)
        {
            byte* namePtr = _sdl.GetAudioDeviceName(i, 1);
            string name = Marshal.PtrToStringAnsi((IntPtr)namePtr) ?? $"Device {i}";
            Devices.Add(new MicrophoneDevice { Name = name, DeviceNumber = i });
        }
        // Don't set SelectedDevice here - let LoadSavedMicrophoneSelection handle it
    }

    [RelayCommand]
    private async Task ToggleTranscriptionAsync()
    {
        if (IsTranscribing)
        {
            await StopTranscriptionAsync();
        }
        else
        {
            await StartTranscriptionAsync();
        }
    }

    private async Task StartTranscriptionAsync()
    {
        if (SelectedDevice == null || _sdl == null) return;
        if (SelectedModel == null)
        {
            BuildModels();
            RefreshInstalledModels();
            SelectedModel = Models.FirstOrDefault();
        }

        try
        {
            string modelPath = SelectedModel!.FileName;
            if (!File.Exists(modelPath))
            {
                TranscriptionText = $"Downloading model: {SelectedModel.Name}...";
                using var httpClient = new HttpClient();
                var downloader = new WhisperGgmlDownloader(httpClient);
                using var modelStream = await downloader.GetGgmlModelAsync(SelectedModel.Type);
                using var fileWriter = File.OpenWrite(modelPath);
                await modelStream.CopyToAsync(fileWriter);
                RefreshInstalledModels();
            }

            _factory?.Dispose();
            
            // Create factory - automatic runtime selection will prioritize Vulkan if available
            _factory = WhisperFactory.FromPath(modelPath);
            
            // Log which runtime is being used (for debugging)
            if (SelectedGpuOption == "Vulkan (GPU)")
            {
                System.Console.WriteLine("[DEBUG_LOG] Attempting to use Vulkan GPU acceleration (automatic selection)");
            }
            else
            {
                System.Console.WriteLine("[DEBUG_LOG] Using CPU processing (automatic selection)");
            }

            _processor?.Dispose();
            
            // Determine language based on model type and selection
            string language = SelectedModel.Type.ToString().Contains("En") ? "en" : (SelectedLanguage ?? "auto");
            
            _processor = _factory.CreateBuilder()
                .WithLanguage(language)
                .Build();

            _waveStream = new MemoryStream();

            OpenAudioDeviceInternal();

            if (_audioDevice == 0)
            {
                unsafe
                {
                    string error = Marshal.PtrToStringAnsi((IntPtr)_sdl.GetError()) ?? "Unknown SDL error";
                    TranscriptionText = $"Error opening audio device: {error}";
                }
                return;
            }

            _sdl.PauseAudioDevice(_audioDevice, 0); // 0 means unpause
            System.Console.WriteLine("[DEBUG_LOG] Audio device unpaused.");
            IsTranscribing = true;
            TranscriptionText = "Listening...\n";
        }
        catch (Exception ex)
        {
            TranscriptionText = $"Error: {ex.Message}";
        }
    }

    private unsafe void OpenAudioDeviceInternal()
    {
        if (_sdl == null || SelectedDevice == null) return;

        AudioSpec desired = new AudioSpec
        {
            Freq = 16000,
            Format = Sdl.AudioS16Sys,
            Channels = 1,
            Samples = 4096,
            Callback = PfnAudioCallback.From(_audioCallbackDelegate)
        };

        AudioSpec obtained;
        // Use NULL for device name to open the default device, 
        // as some systems might have issues with the exact name string returned by SDL.
        _audioDevice = _sdl.OpenAudioDevice(SelectedDevice.Name, 1, &desired, &obtained, 0);
        
        if (_audioDevice == 0)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Failed to open {SelectedDevice.Name}, trying default device...");
            _audioDevice = _sdl.OpenAudioDevice((string?)null, 1, &desired, &obtained, 0);
        }

        System.Console.WriteLine($"[DEBUG_LOG] OpenAudioDevice returned ID: {_audioDevice}. Desired Freq: {desired.Freq}, Obtained Freq: {obtained.Freq}, Obtained Format: {obtained.Format}");
    }

    private unsafe void AudioCallbackInternal(void* userdata, byte* stream, int len)
    {
        if (_waveStream == null || !IsTranscribing) return;

        // [DEBUG_LOG] AudioCallbackInternal called with len: {len}
        System.Console.WriteLine($"[DEBUG_LOG] AudioCallbackInternal called with len: {len}");
        byte[] buffer = new byte[len];
        Marshal.Copy((IntPtr)stream, buffer, 0, len);
        
        lock (_waveStream)
        {
            _waveStream.Write(buffer, 0, len);

            if (_waveStream.Length > 64000)
            {
                var audioData = _waveStream.ToArray();
                _waveStream.SetLength(0);

                _ = ProcessAudioAsync(audioData);
            }
        }
    }

    private async Task ProcessAudioAsync(byte[] audioData)
    {
        // [DEBUG_LOG] ProcessAudioAsync started with {audioData.Length} bytes
        System.Console.WriteLine($"[DEBUG_LOG] ProcessAudioAsync started with {audioData.Length} bytes");
        
        // Convert S16 PCM to Float PCM (Whisper.net expects 16kHz Mono Float)
        float[] samples = new float[audioData.Length / 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short sample = BitConverter.ToInt16(audioData, i * 2);
            samples[i] = sample / 32768.0f;
        }

        var text = "";
        try 
        {
            if (_processor != null)
            {
                // Whisper.net can process raw float samples directly
                await foreach (var result in _processor.ProcessAsync(samples))
                {
                    var resultText = result.Text;
                    if (resultText.Contains("[BLANK_AUDIO]"))
                    {
                        resultText = resultText.Replace("[BLANK_AUDIO]", "").Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(resultText))
                    {
                        text += resultText + " ";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                TranscriptionText += $"\n[Process Error: {ex.Message}]\n";
            });
            return;
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            // Check for word matches and activate shockers
            await CheckWordsAndActivateShockers(text);
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                TranscriptionText += text;
            });
        }
        else
        {
            // [DEBUG_LOG] ProcessAudioAsync: No text transcribed
            System.Console.WriteLine("[DEBUG_LOG] ProcessAudioAsync: No text transcribed");
        }
    }

    private async Task StopTranscriptionAsync()
    {
        if (_sdl != null && _audioDevice != 0)
        {
            _sdl.CloseAudioDevice(_audioDevice);
            _audioDevice = 0;
        }

        if (_processor != null)
        {
            await _processor.DisposeAsync();
            _processor = null;
        }

        lock (_waveStream ?? new object())
        {
            _waveStream?.Dispose();
            _waveStream = null;
        }

        IsTranscribing = false;
    }

    private async Task CheckWordsAndActivateShockers(string transcribedText)
    {
        try
        {
            // Load words from database
            using var db = new AppDbContext();
            var words = db.Words.ToList();
            
            if (!words.Any())
            {
                System.Console.WriteLine("[DEBUG_LOG] No words found in database for word detection");
                return;
            }

            // Load enabled shockers from database
            var enabledShockers = db.Settings
                .Where(s => s.Key.StartsWith("ShockerState_") && s.Value == "True")
                .ToList();

            if (!enabledShockers.Any())
            {
                System.Console.WriteLine("[DEBUG_LOG] No enabled shockers found");
                return;
            }

            // Convert enabled shocker settings to shocker IDs
            var enabledShockerIds = enabledShockers
                .Select(s => s.Key.Replace("ShockerState_", ""))
                .ToList();

            System.Console.WriteLine($"[DEBUG_LOG] Found {enabledShockerIds.Count} enabled shockers: {string.Join(", ", enabledShockerIds)}");

            // Check if any word matches the transcribed text (case-insensitive)
            var transcribedTextLower = transcribedText.ToLower();
            var matchedWords = words.Where(w => transcribedTextLower.Contains(w.Text.ToLower())).ToList();

            if (matchedWords.Any())
            {
                System.Console.WriteLine($"[DEBUG_LOG] Word matches found: {string.Join(", ", matchedWords.Select(w => w.Text))}");
                System.Console.WriteLine($"[DEBUG_LOG] Activating {enabledShockerIds.Count} enabled shockers");

                // Use the first matched word's duration and intensity
                var triggerWord = matchedWords.First();
                System.Console.WriteLine($"[DEBUG_LOG] Using word '{triggerWord.Text}' - Duration: {triggerWord.Duration}ms, Intensity: {triggerWord.Intensity}");

                // Activate all enabled shockers with word-specific settings
                foreach (var shockerId in enabledShockerIds)
                {
                    await ActivateShocker(shockerId, triggerWord.Duration, triggerWord.Intensity);
                }
            }
            else
            {
                System.Console.WriteLine($"[DEBUG_LOG] No word matches found in: '{transcribedText}'");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Error in CheckWordsAndActivateShockers: {ex.Message}");
        }
    }

    private async Task ActivateShocker(string shockerId, int duration, int intensity)
    {
        try
        {
            // Create control request to activate the shocker
            var controlRequest = new ControlRequest
            {
                CustomName = "VoiceShock Activation",
                Shocks = new []
                {
                    new Control
                    {
                        Id = Guid.Parse(shockerId),
                        Type = ControlType.Shock,
                        Duration = (ushort)duration, // Cast int to ushort
                        Intensity = (byte)intensity // Cast int to byte
                    }
                }
            };

            var response = await OpenShockClient.ApiClient.ControlShocker(controlRequest);

            if (response.IsT0)
            {
                System.Console.WriteLine($"[DEBUG_LOG] Successfully activated shocker {shockerId} - Duration: {duration}ms, Intensity: {intensity}");
            }
            else if (response.IsT1)
            {
                var errorType = response.AsT1.Value;
                System.Console.WriteLine($"[DEBUG_LOG] Error activating shocker {shockerId}: {errorType.GetType().Name}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Exception activating shocker {shockerId}: {ex.Message}");
        }
    }

    private void StopTranscriptionSync()
    {
        if (_sdl != null && _audioDevice != 0)
        {
            _sdl.CloseAudioDevice(_audioDevice);
            _audioDevice = 0;
        }

        // Try to dispose asynchronously but wait for it to complete
        if (_processor != null)
        {
            try
            {
                _processor.DisposeAsync().AsTask().Wait();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[DEBUG_LOG] Error disposing processor: {ex.Message}");
            }
            _processor = null;
        }

        lock (_waveStream ?? new object())
        {
            _waveStream?.Dispose();
            _waveStream = null;
        }

        IsTranscribing = false;
    }

    protected override void OnDispose()
    {
        StopTranscriptionSync();
        _sdl?.Quit();
        base.OnDispose();
    }
}
