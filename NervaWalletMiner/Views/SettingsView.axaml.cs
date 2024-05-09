using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects.Constants;
using System;

namespace NervaWalletMiner.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");
            tbxMiningAddress.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress;
        }

        public void SaveSettingsClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool isChanged = false;

                var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");
                var cbxCoin = this.Get<ComboBox>("cbxCoin");

                if (!string.IsNullOrEmpty(tbxMiningAddress.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress != tbxMiningAddress.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = tbxMiningAddress.Text;
                    isChanged = true;
                }

                // TODO: Do this in a better way!
                string newCoin = Coin.XNV;                
                if (cbxCoin.SelectionBoxItem.ToString().ToLower().Contains(Coin.XMR))
                {
                    newCoin = Coin.XMR;
                }
                if(newCoin != GlobalData.AppSettings.ActiveCoin)
                {
                    GlobalData.AppSettings.ActiveCoin = newCoin;
                    isChanged = true;
                    GlobalMethods.SetCoin(newCoin);
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