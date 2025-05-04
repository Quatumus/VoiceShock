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
using Avalonia.Controls;
using System.Diagnostics;

namespace VoiceShock.ViewModels;

public partial class WordEditViewModel : ViewModelBase
{
    
    public int WordId { get; }
    public string WordText { get; }
    
    [ObservableProperty]
    private ObservableCollection<string> _shockers = new();
    
    public WordEditViewModel(int wordId, string wordText)
    {
        WordId = wordId;
        WordText = wordText;
        LoadShockers();
    }
    
    private void LoadShockers()
    {
        Shockers.Clear();
        var list = DatabaseHelper.GetShockersForWord(WordId);
        foreach (var s in list)
        {
            Shockers.Add($"{s.ShockerID} | Intensity: {s.Intensity} | Duration: {s.Duration}");
        }
    }
    

}