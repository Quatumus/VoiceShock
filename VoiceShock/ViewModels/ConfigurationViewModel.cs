using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceShock.Helpers;
using VoiceShock.Views;

namespace VoiceShock.ViewModels;

public partial class ConfigurationViewModel : ViewModelBase
{
    
    [ObservableProperty]
    private string _wordText = string.Empty;

    public void AddItem()
    {
        if (!string.IsNullOrWhiteSpace(WordText))
        {
            var newWord = new WordEditViewModel(2, WordText);
            Words.Add(newWord);
            WordText = string.Empty; // Clear the input after adding
        }
    }

    public void EditItem(WordEditViewModel word)
    {
        // Logic to edit the word, e.g., open a dialog or modify the text directly
    }

    [ObservableProperty]
    private ObservableCollection<WordEditViewModel> _words = new ();

    public ConfigurationViewModel()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VoiceShock", "VoiceShockData.db3"
        );
        DatabaseHelper.Initialize(dbPath);
        LoadWords();
    }

    [RelayCommand]
    private void AddWord()
    {
        if (!string.IsNullOrWhiteSpace(WordText))
        {
            DatabaseHelper.AddWord(WordText);
            WordText = "";
            LoadWords();
        }
    }

    private void LoadWords()
    {
        Words.Clear();
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(DatabaseHelper.ConnectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Text FROM Words";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Words.Add(new WordEditViewModel(reader.GetInt32(0), reader.GetString(1)));
        }
    }

    [RelayCommand]
    private async Task EditWord(WordEditViewModel word)
    {
        var window = new WordEditView
        {
            DataContext = new WordEditViewModel(word.WordId, word.WordText)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow != null) await window.ShowDialog(desktop.MainWindow);
        }

        LoadWords(); // Refresh list in case it changed
    }
}