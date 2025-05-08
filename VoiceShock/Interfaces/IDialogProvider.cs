using VoiceShock.ViewModels;

namespace VoiceShock.Interfaces;

public interface IDialogProvider
{
    DialogViewModel Dialog { get; set; }
}