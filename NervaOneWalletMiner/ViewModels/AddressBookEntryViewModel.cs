namespace NervaOneWalletMiner.ViewModels
{
    internal class AddressBookEntryViewModel : ViewModelBase
    {
        public bool IsNew { get; }
        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Address { get; }
        public string PaymentId { get; }
        public string Title => IsNew ? "Add Address" : "Edit Address";

        public AddressBookEntryViewModel()
        {
            IsNew = true;
            Name = string.Empty;
            Description = string.Empty;
            Address = string.Empty;
            PaymentId = string.Empty;
        }

        public AddressBookEntryViewModel(bool isNew, int id, string name, string description, string address, string paymentId)
        {
            IsNew = isNew;
            Id = id;
            Name = name;
            Description = description;
            Address = address;
            PaymentId = paymentId;
        }
    }
}
