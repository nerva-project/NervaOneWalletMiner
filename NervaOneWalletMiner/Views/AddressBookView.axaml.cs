using Avalonia.Controls;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.DataGrid;

namespace NervaOneWalletMiner.Views
{
    public partial class AddressBookView : UserControl
    {
        public AddressBookView()
        {
            InitializeComponent();

            GlobalMethods.LoadAddressBook();

            if (GlobalData.AddressBook.List.Count == 0)
            {
                // Add blank row so user can add new address
                GlobalData.AddressBook.List.Add(new AddressInfo());
            }
            else
            {
                AddressInfo addressWithHighestId = GetAddressWithHighestId();

                if (!string.IsNullOrEmpty(addressWithHighestId.Address))
                {
                    // Only add new row if there isn't one already based on Address
                    GlobalData.AddressBook.List.Add(new AddressInfo { Id = addressWithHighestId.Id + 1 });
                }
            }

            dtgAddressBook.ItemsSource = GlobalData.AddressBook.List;
        }

        private void AddressBook_RowEditEnded(object? sender, DataGridRowEditEndedEventArgs e)
        {           
            // Just save. Don't need to complicate things
            GlobalMethods.SaveAddressBook();

            // Add blank row if new one was just added
            AddressInfo addressWithHighestId = GetAddressWithHighestId();
            if (!string.IsNullOrEmpty(addressWithHighestId.Address))
            {
                GlobalData.AddressBook.List.Add(new AddressInfo { Id = addressWithHighestId.Id + 1 });
            }
        }

        private AddressInfo GetAddressWithHighestId()
        {
            AddressInfo highestIdAddress = new();

            if(GlobalData.AddressBook.List.Count > 0)
            {
                highestIdAddress = GlobalData.AddressBook.List[0];
                foreach (AddressInfo address in GlobalData.AddressBook.List)
                {
                    if (address.Id > highestIdAddress.Id)
                    {
                        highestIdAddress = address;
                    }
                }
            }

            return highestIdAddress;
        }
    }
}