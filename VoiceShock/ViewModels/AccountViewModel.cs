using System.Reflection.Metadata.Ecma335;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenShock.SDK.CSharp;
using VoiceShock.Config;
using VoiceShock.Helpers;
using OpenShock.SDK.CSharp.Utils;
using VoiceShock.Data;

namespace VoiceShock.ViewModels;

public partial class AccountViewModel() : PageViewModel(ApplicationPageNames.Account)
{
    public Uri Backend { get => AccountConfig.Backend; set => AccountConfig.Backend = value; }
    public string? Password { get => AccountConfig.Password; set => AccountConfig.Password = value; }
    public string? Email { get => AccountConfig.Email; set => AccountConfig.Email = value; }
    public string? Token { get => AccountConfig.Token; set => AccountConfig.Token = value; }
    
    public bool IsLoggedIn { get => true; set => IsLoggedIn = value; }
    public bool IsLoggedOut { get => false; set => IsLoggedOut = value; }
    
    private object _currentPanel = TemplateKey.Login;
    public object CurrentPanel
    {
        get => _currentPanel;
        set
        {
            _currentPanel = value;
            OnPropertyChanged();
        }
    }
    
    private bool _isLoginVisible;
    public bool IsLoginVisible
    {
        get => _isLoginVisible;
        set
        {
            _isLoginVisible = value;
            _isLogoutVisible = !value;
            CurrentPanel = value ? TemplateKey.Login : TemplateKey.Logout;
            OnPropertyChanged();
        }
    }

    private bool _isLogoutVisible;
    public bool IsLogoutVisible
    {
        get => _isLogoutVisible;
        set
        {
            _isLogoutVisible = value;
            _isLoginVisible = !value;
            CurrentPanel = value ? TemplateKey.Logout : TemplateKey.Login;
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private void Login()
    {

        //AccountHelper.client.GetSelf();
        
        
        _ = AccountHelper.SaveItemAsync();
    }

    [RelayCommand]
    private void Logout()
    {
        
    }

    public enum TemplateKey
    {
        Login,
        Logout
    }
}