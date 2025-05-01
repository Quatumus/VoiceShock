using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using VoiceShock.Config;
using VoiceShock.Helpers;
using VoiceShock.Views;

namespace VoiceShock.ViewModels;

public partial class WordEditViewModel : ObservableObject
{
    public int WordId { get; }
    public string WordText { get; }

    [ObservableProperty]
    private ObservableCollection<string> shockers = new();

    public WordEditViewModel(int wordId, string wordText)
    {
        WordId = wordId;
        WordText = wordText;
        LoadShockers();
    }
    
    public void Show() => new WordEditView().Show();
    
    public void Close() => new WordEditView().Close();
    

    private void LoadShockers()
    {
        Shockers.Clear();
        var list = DatabaseHelper.GetShockersForWord(WordId);
        foreach (var s in list)
        {
            Shockers.Add($"{s.ShockerID} | Intensity: {s.Intensity} | Duration: {s.Duration}");
        }
    }
    
    [RelayCommand]
    private async Task EditWord(WordEditViewModel word)
    {
        var window = new WordEditView
        {
            DataContext = new WordEditViewModel(word.WordId, word.WordText)
        };

        // Show the window as a dialog if we are running in a classic desktop windowing environment
        // (i.e. not in a mobile or web context).
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Make sure the main window exists before showing the dialog
            if (desktop.MainWindow != null) await window.ShowDialog(desktop.MainWindow);
        }

        //window.Show(this);

        LoadShockers(); // Refresh list in case it changed
    }
}
