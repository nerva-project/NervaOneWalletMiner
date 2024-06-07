using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            try
            {
                InitializeComponent();

                var cbxThemeVariants = this.Get<ComboBox>("cbxThemeVariants");
                cbxThemeVariants.SelectedItem = Application.Current!.RequestedThemeVariant;
                cbxThemeVariants.SelectionChanged += (sender, e) =>
                {
                    if (cbxThemeVariants.SelectedItem is ThemeVariant themeVariant)
                    {
                        Application.Current!.RequestedThemeVariant = themeVariant;
                    }
                };

                // TODO: Should probably come up with a simpler way to do this
                ComboBoxItem selectedCoin = (ComboBoxItem)cbxCoin.Items[0]!;
                foreach (ComboBoxItem? coin in cbxCoin.Items)
                {
                    if (coin!.Name!.Equals(GlobalData.AppSettings.ActiveCoin))
                    {
                        selectedCoin = coin;
                    }
                }
                cbxCoin.SelectedValue = selectedCoin;
            }
            catch (Exception ex)
            {
                Logger.LogException("SET.CONS", ex);
            }            
        }

        public void SaveSettingsClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool isChanged = false;

                if (cbxThemeVariants.SelectedItem is ThemeVariant themeVariant)
                {
                    if(GlobalData.AppSettings.Theme != themeVariant.Key.ToString())
                    {
                        GlobalData.AppSettings.Theme = themeVariant.Key.ToString()!;
                        isChanged = true;
                    }
                }

                string selectedCoin = ((ComboBoxItem)cbxCoin.SelectedItem!).Name!;
                if (!selectedCoin.Equals(GlobalData.AppSettings.ActiveCoin))
                {
                    GlobalData.AppSettings.ActiveCoin = selectedCoin;

                    // Close wallet process becasue you're switching to different coin
                    WalletProcess.ForceClose();

                    GlobalData.AppSettings.ActiveCoin = selectedCoin;
                    isChanged = true;
                    GlobalMethods.SetCoin(selectedCoin);

                    // TODO: Need to force Settings screen refersh
                }

                if(isChanged)
                {
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("SET.SSC1", ex);
            }
        }
    }
}