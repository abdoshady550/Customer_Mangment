using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Test.Domain
{
    public class AddressTest
    {
        [Fact]
        public void CreateAddress_ShouldRetunError_WhenValueIsNullOrWhiteSpace()
        {
            string firstValue = null;
            string secondValue = "";

            var firstaddressResult = Address.CreateAddress(AdressType.Home, firstValue);
            var secondaddressResult = Address.CreateAddress(AdressType.Home, secondValue);

            Assert.False(firstaddressResult.IsSuccess);
            Assert.False(secondaddressResult.IsSuccess);
            Assert.Equal("Invalide_Address", firstaddressResult.TopError.Code);

        }
    }
}
