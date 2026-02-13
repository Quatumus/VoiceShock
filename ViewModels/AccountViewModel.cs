using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceShock.Data;

namespace VoiceShock.ViewModels;

public partial class AccountViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _email;

    public AccountViewModel()
    {
        LoadUserInfo();
    }

    private async void LoadUserInfo()
    {
        try
        {
            var response = await OpenShockClient.ApiClient.GetSelf();
            if (response.IsT0)
            {
                var user = response.AsT0;
                Username = user.Value.Name;
                Email = user.Value.Email;
            }
        }
        catch
        {
            Username = "Error loading user";
        }
    }

    [RelayCommand]
    private void Logout()
    {
        using var db = new AppDbContext();
        var tokenSetting = db.Settings.FirstOrDefault(s => s.Key == "OpenShockToken");
        if (tokenSetting != null)
        {
            db.Settings.Remove(tokenSetting);
            db.SaveChanges();
        }
        
        // Notify UI to update navigation and switch view
        if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow?.DataContext is MainWindowViewModel mwvm)
        {
            mwvm.IsLoggedIn = false;
            mwvm.NavigateToLogin();
        }
    }
}
