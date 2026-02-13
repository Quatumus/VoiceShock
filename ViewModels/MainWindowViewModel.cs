using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceShock.Data;

namespace VoiceShock.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage;

    [ObservableProperty]
    private bool _isLoggedIn;

    public MainWindowViewModel()
    {
        _currentPage = new LoginViewModel();
        // Fire and forget, but it's okay for startup logic that updates the UI
        _ = CheckLoginStatusAsync();
    }

    private async Task CheckLoginStatusAsync()
    {
        using var db = new AppDbContext();
        var tokenSetting = db.Settings.FirstOrDefault(s => s.Key == "OpenShockToken");
        
        if (tokenSetting != null && !string.IsNullOrWhiteSpace(tokenSetting.Value))
        {
            OpenShockClient.Initialize(tokenSetting.Value);
            var response = await OpenShockClient.ApiClient.GetSelf();
            if (response.IsT0)
            {
                IsLoggedIn = true;
                CurrentPage = new AccountViewModel();
            }
            else
            {
                // Invalid token, remove it
                db.Settings.Remove(tokenSetting);
                await db.SaveChangesAsync();
                IsLoggedIn = false;
                CurrentPage = new LoginViewModel();
            }
        }
        else
        {
            IsLoggedIn = false;
            CurrentPage = new LoginViewModel();
        }
    }

    [RelayCommand]
    public void NavigateToControlPanel()
    {
        CurrentPage = new ControlPanelViewModel();
    }

    [RelayCommand]
    public void NavigateToWords()
    {
        CurrentPage = new WordManagementViewModel();
    }

    [RelayCommand]
    public void NavigateToLogin()
    {
        if (IsLoggedIn)
        {
            CurrentPage = new AccountViewModel();
        }
        else
        {
            CurrentPage = new LoginViewModel();
        }
    }

    [RelayCommand]
    public void NavigateToAccount()
    {
        CurrentPage = new AccountViewModel();
    }

    [RelayCommand]
    public void NavigateToShockers()
    {
        CurrentPage = new ShockersViewModel();
    }
}