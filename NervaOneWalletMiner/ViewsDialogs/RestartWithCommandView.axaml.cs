using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects;
using System;
using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class RestartWithCommandView : UserControl
    {
        public RestartWithCommandView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                Logger.LogException("RCD.CONS", ex);
            }
        }

        public void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var tbxRestartOptions = this.Get<TextBox>("tbxRestartOptions");

                DialogResult result = new()
                {
                    IsOk = true,
                    RestartOptions = tbxRestartOptions.Text!
                };

                DialogService.Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("RCD.OKBC", ex);
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

                DialogService.Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("RCD.CLBC", ex);
            }
        }
    }
}
