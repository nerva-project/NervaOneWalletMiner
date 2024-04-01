using Avalonia.Controls;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NervaWalletMiner.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            /*
            List<Connection> myConnections = [
                new Connection { Address = "rob", Height = 888, State = "nothing", LiveTime = "3:3:3", IsIncoming = true },
                new Connection { Address = "rob2", Height = 777, State = "nothing 2", LiveTime = "4:4:4", IsIncoming = false }];


            var dg1 = this.Get<DataGrid>("dgConnections");
            dg1.IsReadOnly = true;
            dg1.ItemsSource = myConnections;
            */
        }
    }
}