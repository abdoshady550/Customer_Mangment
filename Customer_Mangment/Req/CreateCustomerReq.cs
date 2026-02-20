namespace Customer_Mangment.Req
{
    public class CreateCustomerReq
    {
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public List<CreateAddressReq> Adresses { get; set; } = new();
    }

}
