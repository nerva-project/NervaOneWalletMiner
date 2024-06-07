using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System;
using System.Collections.Generic;
using System.IO;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class OpenWalletView : Window
    {
        public OpenWalletView()
        {
            try
            {
                InitializeComponent();

                var walletName = this.Get<ComboBox>("cbxWalletName");

                walletName.ItemsSource = GetWalletFileNames();
                walletName.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.LogException("OWD.CONS", ex);
            }
        }

        public List<string> GetWalletFileNames()
        {
            List<string> walletFiles = [];
            FileInfo[] files;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(GlobalMethods.GetWalletDir());
                files = dir.GetFiles("*.cache", SearchOption.TopDirectoryOnly);

                if(files.Length == 0)
                {
                    // TODO: XMR wallet files do not have extensions. Make this coin specific setting
                    files = dir.GetFiles("*.", SearchOption.TopDirectoryOnly);
                }

                if (files.Length > 0)
                {
                    foreach (FileInfo file in files)
                    {
                        walletFiles.Add(Path.GetFileNameWithoutExtension(file.FullName));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("OWD.GWFN", ex);
            }

            return walletFiles;
        }

        public void OkButtonClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var cbxWalletName = this.Get<ComboBox>("cbxWalletName");
                var tbxPassword = this.Get<TextBox>("tbxPassword");

                if (string.IsNullOrEmpty(cbxWalletName.SelectedValue!.ToString()) || string.IsNullOrEmpty(tbxPassword.Text))
                {
                    // TODO:  Let user know that both Wallet Name and Password are required

                }
                else
                {
                    DialogResult result = new()
                    {
                        IsOk = true,
                        WalletName = cbxWalletName.SelectedValue.ToString()!,
                        WalletPassword = tbxPassword.Text
                    };

                    Close(result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("OWD.OKBC", ex);
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
                Logger.LogException("OWD.CLBC", ex);
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
                Logger.LogException("OWD.SHPC", ex);
            }
        }
    }
}