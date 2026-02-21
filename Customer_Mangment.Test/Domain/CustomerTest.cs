using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Test.Domain
{
    public class CustomerTest
    {
        //CreateCustomer
        [Fact]
        public void CreateCusomer_ShouldRetunError_WhenNameIsNullOrWhiteSpace()
        {
            var addresses = new List<(AdressType type, string value)>();

            string firstName = null;
            string secoundName = "";

            var firstcustomerResult = Customer.CreateCustomer(firstName, "01000000000", addresses);
            var secoundcustomerResult = Customer.CreateCustomer(secoundName, "01000000000", addresses);

            Assert.False(firstcustomerResult.IsSuccess);
            Assert.False(secoundcustomerResult.IsSuccess);
            Assert.Equal("Invalide_Name", firstcustomerResult.TopError.Code);
        }
        [Fact]
        public void CreateCusomer_ShouldRetunError_WhenMobileIsNullOrWhiteSpace()
        {
            var addresses = new List<(AdressType type, string value)>();

            string firstMobile = null;
            string secoundMobile = "";

            var firstcustomerResult = Customer.CreateCustomer("a", firstMobile, addresses);
            var secoundcustomerResult = Customer.CreateCustomer("a", secoundMobile, addresses);

            Assert.False(firstcustomerResult.IsSuccess);
            Assert.False(secoundcustomerResult.IsSuccess);
            Assert.Equal("Invalide_Mobile", firstcustomerResult.TopError.Code);
        }
        //AddAddress
        [Fact]
        public void UpdateCustomer_ShouldRetunError_WhenAddDuplicateAddressType()
        {
            var addresses = new List<(AdressType type, string value)>();
            var customerResult = Customer.CreateCustomer("Ahmed", "01000000000", addresses);
            var customer = customerResult.Value;


            var firstAddress = customer.AddAddress(AdressType.Home, "Cairo");
            var secondAddress = customer.AddAddress(AdressType.Home, "Giza");

            Assert.True(firstAddress.IsSuccess);
            Assert.False(secondAddress.IsSuccess);
            Assert.Equal("DuplicateAddressType", secondAddress.TopError.Code);
        }
    }
}
