using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects;
using System;
using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class RestoreFromSeedView : Window
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public RestoreFromSeedView()
        {
            try
            {
                InitializeComponent();
                Icon = GlobalMethods.GetWindowIcon();

                var cbxLanguage = this.Get<ComboBox>("cbxLanguage");

                cbxLanguage.ItemsSource = GlobalMethods.GetSupportedLanguages();
                cbxLanguage.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.LogException("RSD.CONS", ex);
            }
        }

        public void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var tbxSeedPhrase = this.Get<TextBox>("tbxSeedPhrase");
                var tbxSeedOffset = this.Get<TextBox>("tbxSeedOffset");

                var tbxWalletName = this.Get<TextBox>("tbxWalletName");
                var tbxPassword = this.Get<TextBox>("tbxPassword");

                var cbxLanguage = this.Get<ComboBox>("cbxLanguage");

                if (string.IsNullOrEmpty(tbxSeedPhrase.Text) || string.IsNullOrEmpty(tbxWalletName.Text) || string.IsNullOrEmpty(tbxPassword.Text))
                {
                    MessageBoxView window = new("Restore From Seed", "Seed Phrase, Wallet Name and Password are all required.", true);
                    window.ShowDialog(GetWindow());
                }
                else
                {
                    DialogResult result = new()
                    {
                        IsOk = true,
                        SeedPhrase = tbxSeedPhrase.Text,
                        SeedOffset = tbxSeedOffset.Text!,
                        WalletName = tbxWalletName.Text,
                        WalletPassword = tbxPassword.Text,
                        WalletLanguage = cbxLanguage.SelectedValue == null ? Language.English : cbxLanguage.SelectedValue.ToString()!
                    };

                    Close(result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RSD.OKBC", ex);
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
                Logger.LogException("RSD.CLBC", ex);
            }
        }

        public void ShowHidePasswordButton_Clicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("RSD.SHPC", ex);
            }
        }
    }
}