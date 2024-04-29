using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using NervaWalletMiner.ViewModels;
using ReactiveUI;
using System;
using System.Threading.Tasks;

namespace NervaWalletMiner.Views
{
    public partial class WalletView : UserControl
    {
        public WalletView()
        {
            InitializeComponent();

        }

        public void OpenCloseWalletClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnOpenCloseWallet = this.Get<Button>("btnOpenCloseWallet");

                if (btnOpenCloseWallet.Content!.ToString()!.Equals(WalletStatus.OpenWallet))
                {
                    // Open wallet
                    var window = new OpenWalletView();
                    var response = window.ShowDialog(GetWindow());

                }
                else
                {
                    // Close wallet

                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Hom.SSMC", ex);
            }
        }

        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");
    }
}