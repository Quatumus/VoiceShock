using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VoiceShock.ViewModels;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    public void Dispose()
    {
        OnDispose();
        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose()
    {
    }
}