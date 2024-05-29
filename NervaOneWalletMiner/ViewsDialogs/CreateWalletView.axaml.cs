using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
using System;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class CreateWalletView : Window
    {
        public CreateWalletView()
        {
            InitializeComponent();

            var cbxLanguage = this.Get<ComboBox>("cbxLanguage");

            cbxLanguage.ItemsSource = GlobalMethods.GetSupportedLanguages();
            cbxLanguage.SelectedIndex = 0;
        }

        public void OkButtonClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var tbxWalletName = this.Get<TextBox>("tbxWalletName");
                var tbxPassword = this.Get<TextBox>("tbxPassword");
                var cbxLanguage = this.Get<ComboBox>("cbxLanguage");

                if (string.IsNullOrEmpty(tbxWalletName.Text) || string.IsNullOrEmpty(tbxPassword.Text))
                {
                    // TODO:  Let user know that both Wallet Name and Password are required

                }
                else
                {
                    DialogResult result = new()
                    {
                        IsOk = true,
                        WalletName = tbxWalletName.Text,
                        WalletPassword = tbxPassword.Text,
                        WalletLanguage = cbxLanguage.SelectedValue == null ? Language.English : cbxLanguage.SelectedValue.ToString()!
                    };

                    Close(result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("NWal.OBC", ex);
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
                Logger.LogException("NWal.CBC", ex);
            }
        }

        public void ShowHidePasswordButtonClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var tbxPassword = this.Get<TextBox>("tbxPassword");
                var btnShowHidePassword = this.Get<Button>("btnShowHidePassword");

                if (tbxPassword.RevealPassword)
                {
                    // Reveal was true, so hide
                    tbxPassword.RevealPassword = false;
                    btnShowHidePassword.Content = "Show";
                }
                else
                {
                    // Reveal was false, so show
                    tbxPassword.RevealPassword = true;
                    btnShowHidePassword.Content = "Hide";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("NWal.SHPBC", ex);
            }
        }
    }
}