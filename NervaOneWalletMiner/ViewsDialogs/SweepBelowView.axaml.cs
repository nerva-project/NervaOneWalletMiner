using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System;

namespace NervaOneWalletMiner.ViewsDialogs;

public partial class SweepBelowView : Window
{
    Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

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
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    await new MessageBoxView("Sweep Below", "Amount and address are required", true).ShowDialog(GetWindow());
                });
                return;
            }

            if (!double.TryParse(tbxAmount.Text, out double amount) || amount <= 0)
            {
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    await new MessageBoxView("Sweep Below", "Amount must be a positive number", true).ShowDialog(GetWindow());
                });
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