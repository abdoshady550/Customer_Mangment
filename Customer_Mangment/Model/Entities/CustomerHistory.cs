using Customer_Mangment.Model.Results;

namespace Customer_Mangment.Model.Entities
{
    public sealed class CustomerHistory
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;
        public DateTime UpdatedAt { get; private set; }
        public string UpdatedBy { get; private set; } = string.Empty;
        public string Action { get; private set; } = string.Empty;
        public string OldData { get; private set; } = string.Empty;
        public string NewData { get; private set; } = string.Empty;
        public Customer Customer { get; private set; }
        private CustomerHistory() { }


        private CustomerHistory(Guid customerId, string name, string mobile, string user)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            CreatedAt = DateTime.Now;
            CreatedBy = user;
            UpdatedAt = CreatedAt;
            UpdatedBy = user;
            Action = $"Created new Customer with name :{name} , mobile:{mobile}";
        }
        private CustomerHistory(Guid customerId,
                                string user,
                                string action,
                                DateTime createdAt,
                                string createdBy,
                                string oldCustomer,
                                string newCustomer)
        {
            Id = Guid.NewGuid();
            CustomerId = customerId;
            CreatedAt = createdAt;
            CreatedBy = createdBy;
            UpdatedAt = DateTime.Now;
            UpdatedBy = user;
            Action = action;
            OldData = oldCustomer;
            NewData = newCustomer;
        }
        public static Result<CustomerHistory> CreateCustomerHistory(Guid customerId,
                                                                    string name,
                                                                    string mobile,
                                                                    string user)
        {
            if (Guid.Empty == customerId)
                return Error.NotFound("Invalide_Customer", "Customer cannot be null");
            if (string.IsNullOrEmpty(name))
                return Error.NotFound("Invalide_Customer_Name", "Customer name cannot be null or empty");
            if (string.IsNullOrEmpty(mobile))
                return Error.NotFound("Invalide_Customer_Mobile", "Customer mobile cannot be null or empty");

            return new CustomerHistory(customerId, name, mobile, user);
        }
        public static Result<CustomerHistory> UpdateCustomerHistory(Guid customerId,
                                                                    string user,
                                                                    string action,
                                                                    DateTime createdAt,
                                                                    string createdBy,
                                                                    string oldCustomer,
                                                                    string newCustomer)
        {
            if (Guid.Empty == customerId)
                return Error.NotFound("Invalide_Customer", "Customer cannot be null");

            return new CustomerHistory(customerId, user, action, createdAt, createdBy, oldCustomer, newCustomer);
        }
    }
}
