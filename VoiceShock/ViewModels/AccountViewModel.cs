using System.Reflection.Metadata.Ecma335;
using CommunityToolkit.Mvvm.Input;

namespace VoiceShock.ViewModels;

public partial class AccountViewModel : ViewModelBase
{
    
    public bool IsLoggedIn { get => true; set => IsLoggedIn = value; }

    [RelayCommand]
    private void Login()
    {
        
    }

    [RelayCommand]
    private void Logout()
    {
        
    }
}