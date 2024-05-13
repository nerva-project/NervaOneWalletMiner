using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using System;
using System.Collections.Generic;
using System.IO;

namespace NervaWalletMiner.Views
{
    public partial class OpenWalletView : Window
    {
        public OpenWalletView()
        {
            InitializeComponent();

            var walletName = this.Get<ComboBox>("cbxWalletName");

            walletName.ItemsSource = GetWalletFileNames();
            walletName.SelectedIndex = 0;
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
                Logger.LogException("OWal.GWFN", ex);
            }

            return walletFiles;
        }

        public void OkButtonClicked(object sender, RoutedEventArgs args)
        {
            var cbxWallet = this.Get<ComboBox>("cbxWalletName");
            var tbxPassword = this.Get<TextBox>("tbxPassword");

            DialogResult result = new()
            {
                IsOk = true,
                WalletName = cbxWallet.SelectedValue?.ToString(),
                WalletPassword = tbxPassword.Text
            };

            Close(result);
        }

        public void CancelButtonClicked(object sender, RoutedEventArgs args)
        {
            DialogResult result = new()
            {
                IsCancel = true
            };

            Close(result);

        }
    }
}