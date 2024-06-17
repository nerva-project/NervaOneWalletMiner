using NervaOneWalletMiner.Helpers;
using ReactiveUI;
using System.Diagnostics;
using System;
using System.Windows.Input;

namespace NervaOneWalletMiner.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        public ICommand OpenLogsFolderCommand { get; }

        public SettingsViewModel()
        {
            OpenLogsFolderCommand = ReactiveCommand.Create(OpenLogsFolder);
        }

        private void OpenLogsFolder()
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
                Logger.LogException("SEM.OPLF", ex);
            }
        }
    }
}