using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VoiceShock.ViewModels;

public partial class EditDialogViewModel : DialogViewModel
{

    [ObservableProperty] private string _title = "Edit Word";
    [ObservableProperty] private string _message = "Select shocker to edit settings.";
    [ObservableProperty] private string _confirmText = "Confirm";
    [ObservableProperty] private string _cancelText = "Cancel";
    
    [ObservableProperty] private bool _confirmed;

    [ObservableProperty] private string[]? _shockerList = ["unable", "to fetch", "shockers"];

    [ObservableProperty] private string _shockerId = "unknown";
    [ObservableProperty] private string _name = "unknown";
    [ObservableProperty] private int _intensity = 30;
    [ObservableProperty] private int _duration = 1000;
    [ObservableProperty] private int _mode;
    [ObservableProperty] private int _warning;
    [ObservableProperty] private bool _enabled;
    
    public int WordId { get; }
    public string WordText { get; }
    
    public EditDialogViewModel(int wordId, string wordText)
    {
        //mainViewModel.Dialog = this;
        WordId = wordId;
        WordText = wordText;
        
        Show();
    }


    [RelayCommand]
    private void LoadItem()
    {
        Console.WriteLine(WordText);
    }
    
    
    [RelayCommand]
    private void Confirm()
    {
        Confirmed = true;
        
        Close();
    }
    


    [RelayCommand]
    private void Cancel()
    {
        Confirmed = false;
        Close();
    }
}