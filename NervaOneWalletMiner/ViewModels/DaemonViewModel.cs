using Avalonia.Controls;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.Generic;

namespace NervaOneWalletMiner.ViewModels
{
    internal class DaemonViewModel : ViewModelBase
    {
        public delegate void StartMiningUiAction(Window owner, int miningThreads);
        public event StartMiningUiAction? StartMiningUiEvent;
        public void StartMiningUi(Window owner,int miningThreads)
        {
            StartMiningUiEvent!.Invoke(owner, miningThreads);
        }

        public delegate void StartMiningNonUiAction(int miningThreads);
        public event StartMiningNonUiAction? StartMiningNonUiEvent;
        public void StartMiningNonUi(int miningThreads)
        {
            StartMiningNonUiEvent!.Invoke(miningThreads);
        }


        private string _StartStopMining = StatusMiner.StartMining;
        public string StartStopMining
        {
            get => _StartStopMining;
            set => this.RaiseAndSetIfChanged(ref _StartStopMining, value);
        }

        private bool _IsNumThreadsEnabled = true;
        public bool IsNumThreadsEnabled
        {
            get => _IsNumThreadsEnabled;
            set => this.RaiseAndSetIfChanged(ref _IsNumThreadsEnabled, value);
        }

        private string _NetHeight = "0";
        public string NetHeight
        {
            get => _NetHeight;
            set => this.RaiseAndSetIfChanged(ref _NetHeight, value);
        }

        private string _YourHeight = "0";
        public string YourHeight
        {
            get => _YourHeight;
            set => this.RaiseAndSetIfChanged(ref _YourHeight, value);
        }

        private string _NetHash = "0";
        public string NetHash
        {
            get => _NetHash;
            set => this.RaiseAndSetIfChanged(ref _NetHash, value);
        }

        private string _RunTime = "0:0:0";
        public string RunTime
        {
            get => _RunTime;
            set => this.RaiseAndSetIfChanged(ref _RunTime, value);
        }

        private string _MinerMessage = "";
        public string MinerMessage
        {
            get => _MinerMessage;
            set => this.RaiseAndSetIfChanged(ref _MinerMessage, value);
        }

        private string _YourHash = "0";
        public string YourHash
        {
            get => _YourHash;
            set => this.RaiseAndSetIfChanged(ref _YourHash, value);
        }

        private string _BlockTime = "∞";
        public string BlockTime
        {
            get => _BlockTime;
            set => this.RaiseAndSetIfChanged(ref _BlockTime, value);
        }

        private string _MiningAddress = "";
        public string MiningAddress
        {
            get => _MiningAddress;
            set => this.RaiseAndSetIfChanged(ref _MiningAddress, value);
        }

        private List<Connection> _Connections = new();
        public List<Connection> Connections
        {
            get => _Connections;
            set => this.RaiseAndSetIfChanged(ref _Connections, value);
        }
    }
}