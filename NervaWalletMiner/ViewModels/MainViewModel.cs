using NervaWalletMiner.Helpers;
using ReactiveUI;
using System.Diagnostics;
using System.Windows.Input;

namespace NervaWalletMiner.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to the new cool Nerva Wallet and Miner!";

    public MainViewModel()
    {
        OpenDebugFolderCommand = ReactiveCommand.Create(OpenDebugFolder);
    }

    public ICommand OpenDebugFolderCommand { get; }

    private void OpenDebugFolder()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = GlobalData.LogDir,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}