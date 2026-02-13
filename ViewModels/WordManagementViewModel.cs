using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using VoiceShock.Data;
using VoiceShock.Models;

namespace VoiceShock.ViewModels;

public partial class WordManagementViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Word> _words = new();

    [ObservableProperty]
    private string _newWordText = string.Empty;

    [ObservableProperty]
    private int _newWordDuration = 1000;

    [ObservableProperty]
    private int _newWordIntensity = 50;

    public WordManagementViewModel()
    {
        LoadWords();
    }

    private void LoadWords()
    {
        using var db = new AppDbContext();
        var words = db.Words.ToList();
        Words = new ObservableCollection<Word>(words);
    }

    [RelayCommand]
    private void AddWord()
    {
        if (string.IsNullOrWhiteSpace(NewWordText)) return;

        var word = new Word
        {
            Text = NewWordText,
            Enabled = true,
            Duration = NewWordDuration,
            Intensity = NewWordIntensity
        };

        using var db = new AppDbContext();
        db.Words.Add(word);
        db.SaveChanges();

        Words.Add(word);
        
        // Reset inputs
        NewWordText = string.Empty;
        NewWordDuration = 1000;
        NewWordIntensity = 50;
    }

    [RelayCommand]
    private void RemoveWord(Word? word)
    {
        if (word == null) return;

        using var db = new AppDbContext();
        db.Words.Remove(word);
        db.SaveChanges();

        Words.Remove(word);
    }
}
