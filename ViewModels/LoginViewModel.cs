using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenShock.SDK.CSharp;
using VoiceShock.Data;
using VoiceShock.Models;

namespace VoiceShock.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? _token;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Token))
        {
            StatusMessage = "Please enter a token.";
            return;
        }

        StatusMessage = "Verifying token...";

        try
        {
            OpenShockClient.Initialize(Token);
            
            var response = await OpenShockClient.ApiClient.GetSelf();
            
            if (response.IsT0)
            {
                StatusMessage = "Token verified. Login successful.";
                
                // Save token to database
                using (var db = new AppDbContext())
                {
                    var tokenSetting = db.Settings.FirstOrDefault(s => s.Key == "OpenShockToken");
                    if (tokenSetting == null)
                    {
                        db.Settings.Add(new Setting { Key = "OpenShockToken", Value = Token });
                    }
                    else
                    {
                        tokenSetting.Value = Token;
                    }
                    await db.SaveChangesAsync();
                }

                // Notify UI to update navigation and switch view
                if (App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop &&
                    desktop.MainWindow?.DataContext is MainWindowViewModel mwvm)
                {
                    mwvm.IsLoggedIn = true;
                    mwvm.NavigateToAccount();
                }
            }
            else
            {
                StatusMessage = "Token verification failed (403/401). Please check your token.";
            }
        }
        catch (System.Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}
