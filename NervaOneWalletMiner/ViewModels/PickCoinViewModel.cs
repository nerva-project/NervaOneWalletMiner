using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using System.Collections.Generic;

namespace NervaOneWalletMiner.ViewModels
{
    internal class PickCoinViewModel : ViewModelBase
    {
        public List<CoinListItem> CoinList => GlobalData.CoinList;
    }
}