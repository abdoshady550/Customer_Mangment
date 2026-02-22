using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Req
{
    public class CreateAddressReq
    {
        public AdressType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
    public class AddAddressReq
    {
        public AdressType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
