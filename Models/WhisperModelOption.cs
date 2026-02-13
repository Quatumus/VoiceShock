using CommunityToolkit.Mvvm.ComponentModel;
using Whisper.net.Ggml;

namespace VoiceShock.Models;

public partial class WhisperModelOption : ObservableObject
{
    public WhisperModelOption(GgmlType type, string name, string fileName)
    {
        Type = type;
        Name = name;
        FileName = fileName;
    }

    public GgmlType Type { get; }
    public string Name { get; }
    public string FileName { get; }

    [ObservableProperty]
    private bool _isInstalled;
}
