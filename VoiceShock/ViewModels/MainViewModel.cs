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

    [ObservableProperty] private DialogViewModel _dialog;
   
    public bool VoiceRecognitionIsActive => CurrentPage is VoiceRecognitionViewModel;
    public bool AccountIsActive => CurrentPage is AccountViewModel;
    public bool ConfigurationIsActive => CurrentPage is ConfigurationViewModel;
    
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