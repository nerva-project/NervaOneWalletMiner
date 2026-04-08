using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System;

namespace NervaOneWalletMiner.ViewsDialogs;

public partial class SweepBelowView : Window
{
    public SweepBelowView()
    {
        try
        {
            InitializeComponent();
            Icon = GlobalMethods.GetWindowIcon();
        }
        catch (Exception ex)
        {
            Logger.LogException("SBD.CONS", ex);
        }
    }

    public async void RunButton_Clicked(object sender, RoutedEventArgs args)
    {
        try
        {
            if (string.IsNullOrEmpty(tbxAmount.Text) || string.IsNullOrEmpty(tbxAddress.Text))
            {
                await DialogService.ShowAsync(new MessageBoxView("Sweep Below", "Amount and address are required", true));
                return;
            }

            if (!double.TryParse(tbxAmount.Text, out double amount) || amount <= 0)
            {
                await DialogService.ShowAsync(new MessageBoxView("Sweep Below", "Amount must be a positive number", true));
                return;
            }

            DialogResult result = new()
            {
                IsOk = true,
                SweepBelowAmount = amount,
                SweepBelowAddress = tbxAddress.Text
            };

            Close(result);
        }
        catch (Exception ex)
        {
            Logger.LogException("SBD.RNBC", ex);
        }
    }

    public void CancelButton_Clicked(object sender, RoutedEventArgs args)
    {
        try
        {
            DialogResult result = new()
            {
                IsCancel = true
            };

            Close(result);
        }
        catch (Exception ex)
        {
            Logger.LogException("SBD.CLBC", ex);
        }
    }
}