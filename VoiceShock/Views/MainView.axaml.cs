using Avalonia.Controls;
using VoiceShock.Helpers;

namespace VoiceShock.Views;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();

        _ = AccountHelper.LoadAsync();
        
    }
}