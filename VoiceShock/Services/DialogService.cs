using VoiceShock.Interfaces;
using VoiceShock.ViewModels;
using System.Threading.Tasks;

namespace VoiceShock.Services;

public class DialogService
{
    public async Task ShowDialog<THost, TDialogViewModel>(THost host, TDialogViewModel dialogViewModel)
        where TDialogViewModel : DialogViewModel
        where THost : IDialogProvider
    {
        // Set host dialog to provided one
        host.Dialog = dialogViewModel;
        dialogViewModel.Show();

        // Wait for dialog to close
        await dialogViewModel.WaitAsnyc();
    }
}