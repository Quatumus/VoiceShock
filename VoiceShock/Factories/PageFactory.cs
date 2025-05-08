using VoiceShock.Data;
using VoiceShock.ViewModels;
using System;

namespace VoiceShock.Factories;

public class PageFactory(Func<Type, PageViewModel> factory)
{
    public PageViewModel GetPageViewModel<T>(Action<T> afterCreation = null)
        where T : PageViewModel
    {
        var viewModel = factory(typeof(T));
        
        afterCreation?.Invoke((T)viewModel);
        
        return viewModel;
    }
}