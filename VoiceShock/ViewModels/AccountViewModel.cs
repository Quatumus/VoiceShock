using System.Reflection.Metadata.Ecma335;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceShock.Config;
using VoiceShock.Helpers;

namespace VoiceShock.ViewModels;

public partial class AccountViewModel : ViewModelBase
{
    
    public Uri Backend { get => AccountConfig.Backend; set => AccountConfig.Backend = value; }
    public string? Password { get => AccountConfig.Password; set => AccountConfig.Password = value; }
    public string? Email { get => AccountConfig.Email; set => AccountConfig.Email = value; }
    public string? Token { get => AccountConfig.Token; set => AccountConfig.Token = value; }
    
    public bool IsLoggedIn { get => true; set => IsLoggedIn = value; }

    [RelayCommand]
    private void Login()
    {
        
        
        _ = AccountHelper.SaveItemAsync();
    }

    [RelayCommand]
    private void Logout()
    {
        
    }
}