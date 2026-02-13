using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenShock.SDK.CSharp;
using OpenShock.SDK.CSharp.Models;
using VoiceShock.Data;
using VoiceShock.Models;

namespace VoiceShock.ViewModels;

public partial class ShockersViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Shocker> _shockers = new();

    [ObservableProperty]
    private Shocker? _selectedShocker;

    [ObservableProperty]
    private bool _isLoading;

    public ShockersViewModel()
    {
        _ = LoadShockersAsync();
    }

    private async Task LoadShockersAsync()
    {
        IsLoading = true;
        try
        {
            // Get shockers from OpenShock API
            var response = await OpenShockClient.ApiClient.GetOwnShockers();
            
            if (response.IsT0)
            {
                // Extract shockers from all hubs (ignore hub structure)
                var shockerModels = response.AsT0.Value
                    .SelectMany(hub => hub.Shockers) // Flatten all shockers from all hubs
                    .Select(s => new Shocker
                    {
                        Name = s.Name ?? $"Shocker {s.Id}",
                        ShockerId = s.Id.ToString(),
                        IsActive = false, // We'll load state from database
                        CreatedAt = DateTime.UtcNow, // Use current time since CreatedOn not available
                        LastActivatedAt = DateTime.UtcNow // Use current time since no UpdatedAt available
                    }).ToList();

                Shockers = new ObservableCollection<Shocker>(shockerModels);
                
                // Load saved states from database
                await LoadShockerStatesAsync(shockerModels);
            }
            else if (response.IsT1)
            {
                System.Console.WriteLine($"[DEBUG_LOG] Error loading shockers: {response.AsT1.ToString()}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Exception loading shockers: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadShockerStatesAsync(IEnumerable<Shocker> shockers)
    {
        using var db = new AppDbContext();
        var savedStates = db.Settings.Where(s => s.Key.StartsWith("ShockerState_")).ToList();
        
        System.Console.WriteLine($"[DEBUG_LOG] Found {savedStates.Count} saved shocker states in database");
        
        foreach (var state in savedStates)
        {
            var shockerId = state.Key.Replace("ShockerState_", "");
            var shocker = shockers.FirstOrDefault(s => s.ShockerId == shockerId);
            if (shocker != null)
            {
                shocker.IsActive = bool.TryParse(state.Value, out var isActive) && isActive;
                System.Console.WriteLine($"[DEBUG_LOG] Loaded state for shocker {shockerId}: {shocker.IsActive}");
            }
            else
            {
                System.Console.WriteLine($"[DEBUG_LOG] No shocker found for saved state: {shockerId}");
            }
        }
        
        // Refresh the collection to notify UI of changes
        var currentShockers = Shockers.ToList();
        Shockers.Clear();
        foreach (var shocker in currentShockers)
        {
            Shockers.Add(shocker);
        }
    }

    [RelayCommand]
    private async Task RefreshShockersAsync()
    {
        await LoadShockersAsync();
    }

    [RelayCommand]
    private async Task ToggleShockerAsync(Shocker? shocker)
    {
        if (shocker == null) return;

        System.Console.WriteLine($"[DEBUG_LOG] ToggleShockerAsync called for {shocker.Name} (ID: {shocker.ShockerId})");

        try
        {
            // Toggle local state immediately for responsive UI
            shocker.IsActive = !shocker.IsActive;
            shocker.LastActivatedAt = DateTime.UtcNow;
            
            System.Console.WriteLine($"[DEBUG_LOG] State toggled to: {shocker.IsActive}");
            
            // Save state to database only (no API calls)
            await SaveShockerStateAsync(shocker);
            
            System.Console.WriteLine($"[DEBUG_LOG] Toggled shocker {shocker.Name}: {shocker.IsActive} (local state only)");
        }
        catch (Exception ex)
        {
            // Error - revert state
            shocker.IsActive = !shocker.IsActive;
            System.Console.WriteLine($"[DEBUG_LOG] Exception toggling shocker: {ex.Message}");
            System.Console.WriteLine($"[DEBUG_LOG] Exception details: {ex}");
        }
    }

    private async Task SaveShockerStateAsync(Shocker shocker)
    {
        using var db = new AppDbContext();
        
        // Check if Settings table exists and has data
        try
        {
            var allSettings = db.Settings.ToList();
            System.Console.WriteLine($"[DEBUG_LOG] Total settings in database: {allSettings.Count}");
            foreach (var s in allSettings.Take(5)) // Show first 5 to avoid spam
            {
                System.Console.WriteLine($"[DEBUG_LOG] Existing setting: {s.Key} = {s.Value}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Error accessing Settings table: {ex.Message}");
        }
        
        var setting = db.Settings.FirstOrDefault(s => s.Key == $"ShockerState_{shocker.ShockerId}");
        
        System.Console.WriteLine($"[DEBUG_LOG] Saving state for shocker {shocker.ShockerId}: {shocker.IsActive}");
        
        if (setting == null)
        {
            setting = new Setting 
            { 
                Key = $"ShockerState_{shocker.ShockerId}", 
                Value = shocker.IsActive.ToString() 
            };
            db.Settings.Add(setting);
            System.Console.WriteLine($"[DEBUG_LOG] Created new setting: {setting.Key} = {setting.Value}");
        }
        else
        {
            setting.Value = shocker.IsActive.ToString();
            System.Console.WriteLine($"[DEBUG_LOG] Updated existing setting: {setting.Key} = {setting.Value}");
        }
        
        try
        {
            var changes = await db.SaveChangesAsync();
            System.Console.WriteLine($"[DEBUG_LOG] Database changes saved: {changes} rows affected");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG_LOG] Error saving to database: {ex.Message}");
            System.Console.WriteLine($"[DEBUG_LOG] Exception details: {ex}");
        }
    }
}
