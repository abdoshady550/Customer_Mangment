using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Req
{
    public class UpdateAddressReq
    {
        public Guid AddressId { get; set; }
        public AdressType Type { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
