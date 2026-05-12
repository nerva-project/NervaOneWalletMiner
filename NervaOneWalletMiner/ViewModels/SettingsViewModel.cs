using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace NervaOneWalletMiner.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        public List<CoinListItem> CoinList => GlobalData.CoinList;

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
                    FileName = GlobalData.LogsDir,
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