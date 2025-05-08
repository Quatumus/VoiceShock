using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using VoiceShock.Data;
using VoiceShock.Helpers;

namespace VoiceShock.ViewModels;

public partial class VoiceRecognitionViewModel : PageViewModel
{
    
    
    async void Login() => await AccountHelper.LoadAsync();
    
    
    [ObservableProperty]
    private List<string> _locationPaths;
    
    public VoiceRecognitionViewModel() : base(ApplicationPageNames.VoiceRecognition)
    {
        // TEMP: Remove
        LocationPaths =
        [
            @"C:\Users\Luke\Downloads\TestActions",
            @"C:\Users\Luke\Documents\BatchProcess",
            @"X:\Shared\BatchProcess\Templates"
        ];
    }
    
}