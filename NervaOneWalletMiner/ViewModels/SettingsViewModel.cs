using NervaOneWalletMiner.Helpers;
using ReactiveUI;
using System.Diagnostics;
using System;
using System.Windows.Input;

namespace NervaOneWalletMiner.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        public ICommand OpenDebugFolderCommand { get; }

        public SettingsViewModel()
        {
            OpenDebugFolderCommand = ReactiveCommand.Create(OpenDebugFolder);
        }

        private void OpenDebugFolder()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = GlobalData.LogDir,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.LogException("Mai.ODF", ex);
            }
        }
    }
}