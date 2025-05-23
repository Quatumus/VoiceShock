using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceShock.Data;
using VoiceShock.Helpers;
using VoiceShock.Views;

namespace VoiceShock.ViewModels;

public partial class ConfigurationViewModel() : PageViewModel(ApplicationPageNames.Configuration)
{
    
    [ObservableProperty]
    private string _wordText = string.Empty;

    public void AddItem()
    {
        if (!string.IsNullOrWhiteSpace(WordText))
        {
            var newWord = new EditDialogViewModel(2, WordText);
            Words.Add(newWord);
            WordText = string.Empty; // Clear the input after adding
        }
    }

    [ObservableProperty]
    private ObservableCollection<EditDialogViewModel> _words = new ();

    /*
    public ConfigurationViewModel()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VoiceShock", "VoiceShockData.db3"
        );
        DatabaseHelper.Initialize(dbPath);
        LoadWords();
    }*/

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
            Words.Add(new EditDialogViewModel(reader.GetInt32(0), reader.GetString(1)));
        }
    }
}