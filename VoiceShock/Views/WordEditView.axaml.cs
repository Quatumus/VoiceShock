using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace VoiceShock.Views;

public partial class WordEditView : Window
{
    public WordEditView()
    {
        //InitializeComponent();
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}