using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
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
                    // Open wallet dialog
                    var window = new OpenWalletView();
                    window.ShowDialog(GetWindow()).ContinueWith(DialogClosed);                   
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

        private void DialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if(result.IsOk)
            {
                // Open wallet
            }
            else
            {
                // Cancelled or closed
            }
        }

        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");
    }
}