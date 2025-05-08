using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceShock.Data;
using VoiceShock.Factories;
using VoiceShock.Views;

namespace VoiceShock.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PageFactory _pageFactory;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VoiceRecognitionIsActive))]
    [NotifyPropertyChangedFor(nameof(AccountIsActive))]
    [NotifyPropertyChangedFor(nameof(ConfigurationIsActive))]
    private PageViewModel _currentPage;

    [ObservableProperty] private static DialogViewModel? _dialog;
   
    public bool VoiceRecognitionIsActive => CurrentPage.PageName == ApplicationPageNames.VoiceRecognition;
    public bool AccountIsActive => CurrentPage.PageName == ApplicationPageNames.Account;
    public bool ConfigurationIsActive => CurrentPage.PageName == ApplicationPageNames.Configuration;
    
    
#pragma warning disable CS8618, CS9264
    public MainViewModel()
    {
        CurrentPage = new VoiceRecognitionViewModel();
    }
#pragma warning restore CS8618, CS9264
    
    public MainViewModel(PageFactory pageFactory)
    {
        _pageFactory = pageFactory ?? throw new ArgumentNullException(nameof(pageFactory));
        CurrentPage = _pageFactory.GetPageViewModel<VoiceRecognitionViewModel>();
    }
    
    [RelayCommand]
    private void ShowAccountView() => CurrentPage = _pageFactory.GetPageViewModel<AccountViewModel>();
    
    [RelayCommand]
    private void ShowVoiceRecognitionView() => CurrentPage = _pageFactory.GetPageViewModel<VoiceRecognitionViewModel>();
    
    [RelayCommand]
    private void ShowConfigurationView() => CurrentPage = _pageFactory.GetPageViewModel<ConfigurationViewModel>();
    
}