using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using System;

namespace NervaWalletMiner.Views
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            var nupThreads = this.Get<NumericUpDown>("nupThreads");
            nupThreads.Maximum = GlobalData.CpuThreadCount;

            // TODO: Change this when you save user settings
            nupThreads.Value = GlobalData.DefaultMiningThreads;


            /*
            List<Connection> myConnections = [
                new Connection { Address = "rob", Height = 888, State = "nothing", LiveTime = "3:3:3", IsIncoming = true },
                new Connection { Address = "rob2", Height = 777, State = "nothing 2", LiveTime = "4:4:4", IsIncoming = false }];


            var dg1 = this.Get<DataGrid>("dgConnections");
            dg1.IsReadOnly = true;
            dg1.ItemsSource = myConnections;
            */
        }

        public void StartStopMiningClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnStartStopMining = this.Get<Button>("btnStartStopMining");
                var nupThreads = this.Get<NumericUpDown>("nupThreads");

                if (btnStartStopMining.Content.ToString().ToLower().Equals("stop mining"))
                {
                    // Stop mining
                    btnStartStopMining.Content = "Start Mining";
                    nupThreads.IsEnabled = true;
                }
                else
                {
                    // Start mining
                    btnStartStopMining.Content = "Stop Mining";
                    nupThreads.IsEnabled = false;
                }
                
            }
            catch (Exception ex)
            {
                Logger.LogException("App.SSM", ex);
            }
        }
    }
}