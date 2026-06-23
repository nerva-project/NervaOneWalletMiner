using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Linq;

namespace NervaOneWalletMiner.Views
{
    public partial class CreateWalletView : UserControl
    {
        private bool _isBtcStyle;
        private bool _isSeedSupported;
        private NBitcoin.Mnemonic? _mnemonic;
        private int[] _verifyIndices = [];

        public CreateWalletView()
        {
            try
            {
                InitializeComponent();

                imgCoinIcon.Source = GlobalMethods.GetLogo();

                _isBtcStyle = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle;
                _isSeedSupported = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsRestoreFromSeedSupported;

                if (_isBtcStyle)
                {
                    pnlLanguage.IsVisible = false;
                    lbPassword.Content = "Password (Required)";
                    tbxPassword.Watermark = "Required - encrypt the wallet with a password";
                    btnOk.IsVisible = !_isSeedSupported;
                    btnNext.IsVisible = _isSeedSupported;
                }
                else
                {
                    cbxLanguage.ItemsSource = GlobalMethods.GetSupportedLanguages();
                    cbxLanguage.SelectedIndex = 0;
                }

                Loaded += (_, _) => tbxWalletName.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.CONS", ex);
            }
        }

        public void Password_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                OkButton_Clicked(sender, args);
            }
        }

        // Step 1 → Step 2 (BTC/DASH only)
        public async void NextButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string walletName = tbxWalletName.Text ?? string.Empty;

                if (string.IsNullOrEmpty(walletName))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Wallet Name is required.", true));
                    return;
                }
                else if (walletName.Contains('/') || walletName.Contains('\\') || walletName.Contains(".."))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Wallet Name cannot contain path characters.", true));
                    return;
                }

                if (string.IsNullOrEmpty(tbxPassword.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Password is required.", true));
                    return;
                }

                // Generate 12-word BIP39 mnemonic
                _mnemonic = new NBitcoin.Mnemonic(NBitcoin.Wordlist.English, NBitcoin.WordCount.Twelve);
                string[] words = _mnemonic.Words;
                tbxSeedDisplay.Text = string.Join(" ", words);

                // Pick 3 distinct random positions to verify
                Random rng = Random.Shared;
                System.Collections.Generic.HashSet<int> picked = [];
                while (picked.Count < 3)
                {
                    picked.Add(rng.Next(0, words.Length));
                }
                _verifyIndices = [.. picked.OrderBy(x => x)];

                lbWord1.Content = "Word #" + (_verifyIndices[0] + 1) + ":";
                lbWord2.Content = "Word #" + (_verifyIndices[1] + 1) + ":";
                lbWord3.Content = "Word #" + (_verifyIndices[2] + 1) + ":";

                tbxWord1.Text = string.Empty;
                tbxWord2.Text = string.Empty;
                tbxWord3.Text = string.Empty;

                pnlStep1.IsVisible = false;
                pnlStep2.IsVisible = true;
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.NEXT", ex);
            }
        }

        // Step 2 → Step 3
        public void ConfirmSeed_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                pnlStep2.IsVisible = false;
                pnlStep3.IsVisible = true;
                tbxWord1.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.CNFS", ex);
            }
        }

        // Step 2 → Step 1
        public void BackToStep1_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                _mnemonic = null;
                pnlStep2.IsVisible = false;
                pnlStep1.IsVisible = true;
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.BK1", ex);
            }
        }

        // Step 3 → Step 2
        public void BackToStep2_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                pnlStep3.IsVisible = false;
                pnlStep2.IsVisible = true;
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.BK2", ex);
            }
        }

        // Create button — XMR goes directly here; BTC/DASH arrives from Step 3; BTC-style without seed support goes directly here
        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (_isBtcStyle && !_isSeedSupported)
                {
                    await CreateBtcSimpleWallet();
                }
                else if (_isBtcStyle)
                {
                    await CreateBtcStyleWallet();
                }
                else
                {
                    await CreateXmrStyleWallet();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.OKBC", ex);
            }
        }

        private async System.Threading.Tasks.Task CreateBtcStyleWallet()
        {
            if (_mnemonic == null)
            {
                return;
            }

            string[] words = _mnemonic.Words;

            string w1 = (tbxWord1.Text ?? string.Empty).Trim().ToLowerInvariant();
            string w2 = (tbxWord2.Text ?? string.Empty).Trim().ToLowerInvariant();
            string w3 = (tbxWord3.Text ?? string.Empty).Trim().ToLowerInvariant();

            if (w1 != words[_verifyIndices[0]] ||
                w2 != words[_verifyIndices[1]] ||
                w3 != words[_verifyIndices[2]])
            {
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "One or more words did not match. Check your seed phrase and try again.", true));
                return;
            }

            string walletName = tbxWalletName.Text ?? string.Empty;
            char[] walletPassword = string.IsNullOrEmpty(tbxPassword.Text) ? [] : tbxPassword.Text.ToCharArray();
            char[] seed = string.Join(" ", words).ToCharArray();

            tbxPassword.Text = string.Empty;
            tbxSeedDisplay.Text = string.Empty;
            tbxWord1.Text = string.Empty;
            tbxWord2.Text = string.Empty;
            tbxWord3.Text = string.Empty;

            btnCreateFromSeed.Content = "Creating...";
            btnCreateFromSeed.IsEnabled = false;
            btnBackToStep2.IsEnabled = false;

            Logger.LogDebug("CWV.OKBC", "Creating wallet from seed: " + walletName);

            string walletPasswordHash = Hashing.Hash(walletPassword);

            CreateWalletRequest request = new()
            {
                WalletName = walletName,
                Password = walletPassword,
                Seed = seed
            };

            CreateWalletResponse response = await GlobalData.WalletService.CreateWalletFromSeed(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

            Array.Clear(walletPassword, 0, walletPassword.Length);
            Array.Clear(seed, 0, seed.Length);
            _mnemonic = null;

            if (response.Error.IsError)
            {
                GlobalMethods.WalletClosedOrErrored();
                Logger.LogError("CWV.OKBC", "Failed to create wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Error creating " + walletName + " wallet\r\n" + GlobalMethods.GetRpcErrorMessage(response.Error.Content, response.Error.Message), true));
                btnCreateFromSeed.Content = "Create Wallet";
                btnCreateFromSeed.IsEnabled = true;
                btnBackToStep2.IsEnabled = true;
                return;
            }

            GlobalMethods.WalletJustOpened(walletName);

            if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
            {
                GlobalData.WalletPassProvidedTime = DateTime.Now;
                GlobalData.WalletPasswordHash = walletPasswordHash;
            }

            Logger.LogDebug("CWV.OKBC", "Wallet " + walletName + " created from seed successfully");
            await DialogService.ShowAsync(new MessageBoxView("Create Wallet", walletName + " wallet created and opened successfully!", true));
            UIManager.NavigateToPage(SplitViewPages.Wallet);
        }

        private async System.Threading.Tasks.Task CreateBtcSimpleWallet()
        {
            string walletName = tbxWalletName.Text ?? string.Empty;

            if (string.IsNullOrEmpty(walletName))
            {
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Wallet Name is required.", true));
                return;
            }
            else if (walletName.Contains('/') || walletName.Contains('\\') || walletName.Contains(".."))
            {
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Wallet Name cannot contain path characters.", true));
                return;
            }

            if (string.IsNullOrEmpty(tbxPassword.Text))
            {
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Password is required.", true));
                return;
            }

            char[] walletPassword = tbxPassword.Text.ToCharArray();

            tbxPassword.Text = string.Empty;
            btnOk.Content = "Creating...";
            btnOk.IsEnabled = false;
            btnCancel.IsEnabled = false;
            tbxWalletName.IsEnabled = false;

            Logger.LogDebug("CWV.OKBC", "Creating wallet: " + walletName);

            CreateWalletRequest request = new()
            {
                WalletName = walletName,
                Password = walletPassword
            };

            CreateWalletResponse response = await GlobalData.WalletService.CreateWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

            Array.Clear(walletPassword, 0, walletPassword.Length);

            if (response.Error.IsError)
            {
                GlobalMethods.WalletClosedOrErrored();
                Logger.LogError("CWV.OKBC", "Failed to create wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Error creating " + walletName + " wallet\r\n" + GlobalMethods.GetRpcErrorMessage(response.Error.Content, response.Error.Message), true));
                btnOk.Content = "Create";
                btnOk.IsEnabled = true;
                btnCancel.IsEnabled = true;
                tbxWalletName.IsEnabled = true;
                return;
            }

            GlobalMethods.WalletJustOpened(walletName);

            Logger.LogDebug("CWV.OKBC", "Wallet " + walletName + " created successfully");
            await DialogService.ShowAsync(new MessageBoxView("Create Wallet", walletName + " wallet created and opened successfully!\n\nMake sure to back up your wallet folder. It is located in:\n" + System.IO.Path.Combine(GlobalData.WalletDir, walletName) + "\n\nYou can also use Wallet Setup > Dump Keys to File as an additional backup. To restore, use Wallet Setup > Restore from Dump File.", true));
            UIManager.NavigateToPage(SplitViewPages.Wallet);
        }

        private async System.Threading.Tasks.Task CreateXmrStyleWallet()
        {
            string walletName = tbxWalletName.Text == null ? string.Empty : tbxWalletName.Text;

            if (string.IsNullOrEmpty(walletName) || string.IsNullOrEmpty(tbxPassword.Text))
            {
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Wallet Name and Password are required.", true));
                return;
            }
            else if (walletName.Contains('/') || walletName.Contains('\\') || walletName.Contains(".."))
            {
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Wallet Name cannot contain path characters.", true));
                return;
            }

            char[] walletPassword = tbxPassword.Text.ToCharArray();
            string walletLanguage = cbxLanguage.SelectedValue == null ? Language.English : cbxLanguage.SelectedValue.ToString()!;

            tbxPassword.Text = string.Empty;
            btnOk.Content = "Creating...";
            btnOk.IsEnabled = false;
            btnCancel.IsEnabled = false;
            tbxWalletName.IsEnabled = false;
            cbxLanguage.IsEnabled = false;

            Logger.LogDebug("CWV.OKBC", "Creating wallet: " + walletName);

            string walletPasswordHash = Hashing.Hash(walletPassword);

            CreateWalletRequest request = new()
            {
                WalletName = walletName,
                Password = walletPassword,
                Language = walletLanguage
            };

            CreateWalletResponse response = await GlobalData.WalletService.CreateWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

            Array.Clear(walletPassword, 0, walletPassword.Length);

            if (response.Error.IsError)
            {
                GlobalMethods.WalletClosedOrErrored();
                Logger.LogError("CWV.OKBC", "Failed to create wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Error creating " + walletName + " wallet\r\n" + GlobalMethods.GetRpcErrorMessage(response.Error.Content, response.Error.Message), true));
                btnOk.Content = "Create";
                btnOk.IsEnabled = true;
                btnCancel.IsEnabled = true;
                tbxWalletName.IsEnabled = true;
                cbxLanguage.IsEnabled = true;
                return;
            }

            GlobalMethods.WalletJustOpened(walletName);

            if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
            {
                GlobalData.WalletPassProvidedTime = DateTime.Now;
                GlobalData.WalletPasswordHash = walletPasswordHash;
            }

            Logger.LogDebug("CWV.OKBC", "Wallet " + walletName + " created successfully");
            UIManager.NavigateToDisplayKeysSeed("Wallet created and opened successfully! Save your seed phrase and keys to a safe place. You'll need them to restore your wallet. Keep them private - anyone with access can steal your funds!", SplitViewPages.Wallet);
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("CWV.CNCL", "Create wallet cancelled");
                _mnemonic = null;
                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.CNCL", ex);
            }
        }

        public void ShowHidePassword_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (tbxPassword.RevealPassword)
                {
                    tbxPassword.RevealPassword = false;
                    btnShowHidePassword.Content = "Show";
                }
                else
                {
                    tbxPassword.RevealPassword = true;
                    btnShowHidePassword.Content = "Hide";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.SHPC", ex);
            }
        }
    }
}
