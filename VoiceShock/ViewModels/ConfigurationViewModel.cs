using System;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using VoiceShock.Helpers;

namespace VoiceShock.ViewModels;

public partial class ConfigurationViewModel : ViewModelBase
{
    [ObservableProperty] private string _wordText;
    [ObservableProperty] private string _shockerId;
    [ObservableProperty] private bool _shockerEnabled;
    [ObservableProperty] private int _duration;
    [ObservableProperty] private int _intensity;
    [ObservableProperty] private int _controlType;
    [ObservableProperty] private int _warning;

    [ObservableProperty]
    private ObservableCollection<WordEntry> _words = new();

    [ObservableProperty]
    private ObservableCollection<string> _shockerInfo = new();

    private int _currentWordId = -1;

    public ConfigurationViewModel()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "VoiceShock",
            "VoiceShockData.db3"
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
            LoadWords();
        }
    }

    [RelayCommand]
    private void SelectWord(int wordId)
    {
        _currentWordId = wordId;
        LoadShockers();
    }

    [RelayCommand]
    private void AddShocker()
    {
        if (_currentWordId < 0) return;

        int sid = DatabaseHelper.AddShocker(ShockerId, ShockerEnabled);
        DatabaseHelper.LinkWordToShocker(_currentWordId, sid, Duration, Intensity, ControlType, Warning);
        LoadShockers();
    }

    private void LoadWords()
    {
        Words.Clear();
        using var conn = new SqliteConnection(DatabaseHelper.ConnectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Text FROM Words";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Words.Add(new WordEntry
            {
                Id = reader.GetInt32(0),
                Text = reader.GetString(1)
            });
        }
    }

    private void LoadShockers()
    {
        ShockerInfo.Clear();
        var shockers = DatabaseHelper.GetShockersForWord(_currentWordId);
        foreach (var s in shockers)
        {
            ShockerInfo.Add($"{s.ShockerID} - Enabled: {s.Enabled}, Intensity: {s.Intensity}, Duration: {s.Duration}");
        }
    }

    public partial class WordEntry : ObservableObject
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}