using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects;
using System;
using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class RestartWithCommandView : Window
    {
        public RestartWithCommandView()
        {
            try
            {
                InitializeComponent();
                Icon = GlobalMethods.GetWindowIcon();
            }
            catch (Exception ex)
            {
                Logger.LogException("RCD.CONS", ex);
            }            
        }

        public void OkButtonClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var tbxRestartOptions = this.Get<TextBox>("tbxRestartOptions");

                DialogResult result = new()
                {
                    IsOk = true,
                    RestartOptions = tbxRestartOptions.Text!
                };

                Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("RCD.OKBC", ex);
            }
        }

        public void CancelButtonClicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("RCD.CLBC", ex);
            }
        }
    }
}