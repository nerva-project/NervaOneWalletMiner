using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.ViewModels;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class PickCoinView : UserControl
    {
        public PickCoinView()
        {
            InitializeComponent();

            cbxCoin.SelectedIndex = 0;
        }

        public void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string selectedCoin = ((ComboBoxItem)cbxCoin.SelectedItem!).Name!;
                GlobalMethods.SetCoin(selectedCoin);
                GlobalMethods.SaveConfig();

                GlobalMethods.LoadConfig();

                UIManager.SetUpPages();

                ((MainViewModel)GlobalData.ViewModelPages[SplitViewPages.MainView]).CheckAndGetCliTools();
            }
            catch (Exception ex)
            {
                Logger.LogException("PIC.OKBC", ex);
            }
        }

        public void ExitButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.Shutdown();
            }
            catch (Exception ex)
            {
                Logger.LogException("PIC.EXBC", ex);
            }
        }
    }
}