using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Metadata;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using Avalonia.Markup.Xaml;
using VoiceShock.Factories;
using VoiceShock.Services;
using VoiceShock.ViewModels;
using VoiceShock.Views;

namespace VoiceShock;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection.AddSingleton<MainViewModel>();
        collection.AddTransient<VoiceRecognitionViewModel>();
        collection.AddTransient<ConfigurationViewModel>();
        collection.AddTransient<AccountViewModel>();
        
        collection.AddSingleton<Func<Type, PageViewModel>>(x => type => type switch
        {
            _ when type == typeof(VoiceRecognitionViewModel) => x.GetRequiredService<VoiceRecognitionViewModel>(),
            _ when type == typeof(ConfigurationViewModel) => x.GetRequiredService<ConfigurationViewModel>(),
            _ when type == typeof(AccountViewModel) => x.GetRequiredService<AccountViewModel>(),
            _ => throw new InvalidOperationException($"Page of type {type?.FullName} has no view model"),
        });
        
        collection.AddSingleton<PageFactory>();
        collection.AddSingleton<DialogService>();
        
        var services = collection.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainView
            {
                DataContext = services.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}