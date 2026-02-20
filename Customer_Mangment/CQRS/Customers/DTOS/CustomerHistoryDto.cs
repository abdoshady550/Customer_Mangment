namespace Customer_Mangment.CQRS.Customers.DTOS
{
    public class CustomerHistoryDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string OldData { get; set; } = string.Empty;
        public string NewData { get; set; } = string.Empty;
    }
}
