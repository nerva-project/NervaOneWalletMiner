using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects;
using System;
using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class RestoreFromKeysView : Window
    {
        public RestoreFromKeysView()
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
                var tbxWalletAddress = this.Get<TextBox>("tbxWalletAddress");
                var tbxViewKey = this.Get<TextBox>("tbxViewKey");
                var tbxSpendKey = this.Get<TextBox>("tbxSpendKey");

                var tbxWalletName = this.Get<TextBox>("tbxWalletName");
                var tbxPassword = this.Get<TextBox>("tbxPassword");

                var cbxLanguage = this.Get<ComboBox>("cbxLanguage");

                if (string.IsNullOrEmpty(tbxWalletAddress.Text) || string.IsNullOrEmpty(tbxViewKey.Text) || string.IsNullOrEmpty(tbxSpendKey.Text)
                    || string.IsNullOrEmpty(tbxWalletName.Text) || string.IsNullOrEmpty(tbxPassword.Text))
                {
                    // TODO:  Let user know that required componets are missing

                }
                else
                {
                    DialogResult result = new()
                    {
                        IsOk = true,
                        WalletAddress = tbxWalletAddress.Text,
                        ViewKey = tbxViewKey.Text,
                        SpendKey = tbxSpendKey.Text!,
                        WalletName = tbxWalletName.Text,
                        WalletPassword = tbxPassword.Text,
                        WalletLanguage = cbxLanguage.SelectedValue == null ? Language.English : cbxLanguage.SelectedValue.ToString()!
                    };

                    Close(result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RFKWal.OBC", ex);
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
                Logger.LogException("RFKWal.CBC", ex);
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
                Logger.LogException("RFKWal.SHPBC", ex);
            }
        }
    }
}
