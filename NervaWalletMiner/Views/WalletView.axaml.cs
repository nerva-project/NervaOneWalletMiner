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
    //public partial class WalletView : ReactiveWindow<WalletViewModel>
    public partial class WalletView : UserControl
    {
        public WalletView()
        {
            InitializeComponent();

            //this.WhenActivated(action => action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
        }

        /*
        private async Task DoShowDialogAsync(InteractionContext<OpenWalletViewModel, SelectedWalletViewModel?> interaction)
        {
            var dialog = new OpenWalletView();
            dialog.DataContext = interaction.Input;

            var result = await dialog.ShowDialog<SelectedWalletViewModel?>(this);
            interaction.SetOutput(result);
        }
        */

        public void OpenCloseWalletClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnOpenCloseWallet = this.Get<Button>("btnOpenCloseWallet");

                if (btnOpenCloseWallet.Content!.ToString()!.Equals(WalletStatus.OpenWallet))
                {
                    // Open wallet

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
    }
}