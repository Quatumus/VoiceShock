using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceShock.Views;

namespace VoiceShock.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VoiceRecognitionIsActive))]
    [NotifyPropertyChangedFor(nameof(AccountIsActive))]
    [NotifyPropertyChangedFor(nameof(ConfigurationIsActive))]
    private ViewModelBase _currentPage;
   
    public bool VoiceRecognitionIsActive => CurrentPage == _voiceRecognitionView;
    public bool AccountIsActive => CurrentPage == _accountView;
    public bool ConfigurationIsActive => CurrentPage == _configurationView;
    
    private readonly VoiceRecognitionViewModel _voiceRecognitionView = new ();
    private readonly AccountViewModel _accountView = new (); 
    private readonly ConfigurationViewModel _configurationView = new ();
    
    public MainViewModel()
    {
        CurrentPage = _voiceRecognitionView;
    }
    
    [RelayCommand]
    private void ShowAccountView() => CurrentPage = _accountView;
    
    [RelayCommand]
    private void ShowVoiceRecognitionView() => CurrentPage = _voiceRecognitionView;
    
    [RelayCommand]
    private void ShowConfigurationView() => CurrentPage = _configurationView;
    
}