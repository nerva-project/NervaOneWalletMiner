using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using System;

namespace NervaWalletMiner.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");
            tbxMiningAddress.Text = GlobalData.ApplicationSettings.MiningAddress;
        }

        public void SaveSettingsClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");

                if (!string.IsNullOrEmpty(tbxMiningAddress.Text))
                {
                    GlobalData.ApplicationSettings.MiningAddress = tbxMiningAddress.Text;
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