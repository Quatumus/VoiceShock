using VoiceShock.Config;
using VoiceShock.Helpers;

namespace VoiceShock.ViewModels;

public partial class VoiceRecognitionViewModel : ViewModelBase
{
    
    
    async void Login() => await AccountHelper.LoadAsync();
    
    
    
    
}