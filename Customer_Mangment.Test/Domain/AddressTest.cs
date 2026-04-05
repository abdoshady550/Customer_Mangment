using Customer_Mangment.Model.Entities;
using Customer_Mangment.SharedResources;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace Customer_Mangment.Test.Domain
{
    public class AddressTest
    {
        private static IStringLocalizer<SharedResource> CreateLocalizer()
        {
            var localizer = Substitute.For<IStringLocalizer<SharedResource>>();
            localizer[Arg.Any<string>()].Returns(ci =>
                new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
            return localizer;
        }

        [Fact]
        public void CreateAddress_ShouldReturnError_WhenValueIsNull()
        {
            var localizer = CreateLocalizer();

            var result = Address.CreateAddress(AdressType.Home, null!, localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Address", result.TopError.Code);
        }

        [Fact]
        public void CreateAddress_ShouldReturnError_WhenValueIsEmpty()
        {
            var localizer = CreateLocalizer();

            var result = Address.CreateAddress(AdressType.Home, "", localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Address", result.TopError.Code);
        }

        [Fact]
        public void CreateAddress_ShouldReturnError_WhenValueIsWhiteSpace()
        {
            var localizer = CreateLocalizer();

            var result = Address.CreateAddress(AdressType.Home, "   ", localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Address", result.TopError.Code);
        }

        [Theory]
        [InlineData(AdressType.Home, "123 Main St")]
        [InlineData(AdressType.Work, "456 Office Ave")]
        [InlineData(AdressType.Other, "789 Other Rd")]
        public void CreateAddress_ShouldSucceed_ForAllAddressTypes(AdressType type, string value)
        {
            var localizer = CreateLocalizer();

            var result = Address.CreateAddress(type, value, localizer);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(type, result.Value.Type);
            Assert.Equal(value, result.Value.Value);
        }

        [Fact]
        public void CreateAddress_ShouldAssignNewId_OnSuccess()
        {
            var localizer = CreateLocalizer();

            var result = Address.CreateAddress(AdressType.Home, "Cairo", localizer);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
        }

        [Fact]
        public void UpdateAddress_ShouldUpdateValue_WhenNewValueProvided()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType type, string value)>();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", addresses, localizer).Value;
            customer.AddAddress(AdressType.Home, "Old Address", localizer);
            var address = customer.Addresses.First();

            var result = address.UpdateAddress(null, "New Address", "system");

            Assert.True(result.IsSuccess);
            Assert.Equal("New Address", address.Value);
        }

        [Fact]
        public void UpdateAddress_ShouldUpdateType_WhenNewTypeProvided()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType type, string value)>();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", addresses, localizer).Value;
            customer.AddAddress(AdressType.Home, "Cairo", localizer);
            var address = customer.Addresses.First();

            var result = address.UpdateAddress(AdressType.Work, null, "system");

            Assert.True(result.IsSuccess);
            Assert.Equal(AdressType.Work, address.Type);
        }

        [Fact]
        public void UpdateAddress_ShouldNotChangeValue_WhenNullValueProvided()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType type, string value)>();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", addresses, localizer).Value;
            customer.AddAddress(AdressType.Home, "Cairo", localizer);
            var address = customer.Addresses.First();

            address.UpdateAddress(null, null, "system");

            Assert.Equal("Cairo", address.Value);
        }
    }
}
