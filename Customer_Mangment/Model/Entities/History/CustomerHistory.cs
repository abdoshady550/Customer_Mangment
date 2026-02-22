namespace Customer_Mangment.Model.Entities.History
{
    public class CustomerHistory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}