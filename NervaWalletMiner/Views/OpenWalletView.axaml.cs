using Avalonia.Controls;
using NervaWalletMiner.Helpers;
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
    }
}
