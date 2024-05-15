using NervaOneWalletMiner.Helpers;
using ReactiveUI;
using System.Diagnostics;
using System;
using System.Windows.Input;
using Avalonia.Media.Imaging;

namespace NervaOneWalletMiner.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        // TODO: Figure out how to do this in one place instead of on each view
        private Bitmap _CoinIcon = GlobalData.Logo;
        public Bitmap CoinIcon
        {
            get => _CoinIcon;
            set => this.RaiseAndSetIfChanged(ref _CoinIcon, value);
        }

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