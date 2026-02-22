namespace Customer_Mangment.Model.Entities.History
{
    public class AddressHistory
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public int Type { get; set; }
        public string Value { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}