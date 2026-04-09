using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class SweepBelowView : UserControl
    {
        public SweepBelowView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
            }
            catch (Exception ex)
            {
                Logger.LogException("SBV.CONS", ex);
            }
        }

        public async void RunButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(tbxAmount.Text) || string.IsNullOrEmpty(tbxAddress.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Sweep Below", "Amount and address are required.", true));
                    return;
                }

                if (!double.TryParse(tbxAmount.Text, out double amount) || amount <= 0)
                {
                    await DialogService.ShowAsync(new MessageBoxView("Sweep Below", "Amount must be a positive number.", true));
                    return;
                }

                string address = tbxAddress.Text;

                btnRun.Content = "Running...";
                btnRun.IsEnabled = false;
                btnCancel.IsEnabled = false;
                tbxAmount.IsEnabled = false;
                tbxAddress.IsEnabled = false;

                Logger.LogDebug("SBV.RNBC", "Calling SweepBelow");

                SweepBelowRequest request = new()
                {
                    Amount = amount,
                    WalletAddress = address
                };

                SweepBelowResponse response = await GlobalData.WalletService.SweepBelow(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("SBV.RNBC", "Failed to sweep below | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await DialogService.ShowAsync(new MessageBoxView("Sweep Below", "Error running sweep below\r\n" + response.Error.Message, true));
                    btnRun.Content = "Run Sweep Below";
                    btnRun.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    tbxAmount.IsEnabled = true;
                    tbxAddress.IsEnabled = true;
                    return;
                }

                Logger.LogDebug("SBV.RNBC", "Sweep below returned successfully");
                await DialogService.ShowAsync(new MessageBoxView("Sweep Below", "Sweep below submitted successfully.", true));

                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("SBV.RNBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("SBV.CNCL", "Sweep below cancelled");
                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("SBV.CNCL", ex);
            }
        }
    }
}
