using Avalonia.Controls;
using VoiceShock.Helpers;

namespace VoiceShock.Views;

public partial class WordEdit : Window
{
    public WordEdit()
    {
        InitializeComponent();

        _ = AccountHelper.LoadAsync();
        
    }
}