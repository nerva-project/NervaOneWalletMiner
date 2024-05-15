using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");
            tbxMiningAddress.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress;

            var tbxDaemonDataDir = this.Get<TextBox>("tbxDaemonDataDir");
            tbxDaemonDataDir.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir;

            var cbxThemeVariants = this.Get<ComboBox>("cbxThemeVariants");
            cbxThemeVariants.SelectedItem = Application.Current!.RequestedThemeVariant;
            cbxThemeVariants.SelectionChanged += (sender, e) =>
            {
                if (cbxThemeVariants.SelectedItem is ThemeVariant themeVariant)
                {
                    Application.Current!.RequestedThemeVariant = themeVariant;
                }
            };
        }

        public void SaveSettingsClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool isChanged = false;

                var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");
                var tbxDaemonDataDir = this.Get<TextBox>("tbxDaemonDataDir");
                var cbxThemeVariants = this.Get<ComboBox>("cbxThemeVariants");

                var cbxCoin = this.Get<ComboBox>("cbxCoin");

                if (!string.IsNullOrEmpty(tbxMiningAddress.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress != tbxMiningAddress.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = tbxMiningAddress.Text;
                    isChanged = true;
                }

                if (!string.IsNullOrEmpty(tbxDaemonDataDir.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir != tbxDaemonDataDir.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir = tbxDaemonDataDir.Text;
                    isChanged = true;
                }

                if (cbxThemeVariants.SelectedItem is ThemeVariant themeVariant)
                {
                    if(GlobalData.AppSettings.Theme != themeVariant.Key.ToString())
                    {
                        GlobalData.AppSettings.Theme = themeVariant.Key.ToString()!;
                        isChanged = true;
                    }
                }

                // TODO: Do this in a better way!
                string newCoin = Coin.XNV;                
                if (cbxCoin.SelectionBoxItem.ToString().ToLower().Contains(Coin.XMR))
                {
                    newCoin = Coin.XMR;
                }
                if(newCoin != GlobalData.AppSettings.ActiveCoin)
                {
                    // Close wallet process becasue you're switching to different coin
                    WalletProcess.ForceClose();

                    GlobalData.AppSettings.ActiveCoin = newCoin;
                    isChanged = true;
                    GlobalMethods.SetCoin(newCoin);

                    // TODO: Need to force Settings screen refersh
                }

                if(isChanged)
                {
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("SetV.SSC", ex);
            }
        }
    }
}