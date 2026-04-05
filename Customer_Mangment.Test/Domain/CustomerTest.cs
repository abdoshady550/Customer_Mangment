using Customer_Mangment.Model.Entities;
using Customer_Mangment.SharedResources;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace Customer_Mangment.Test.Domain
{
    public class CustomerTest
    {
        private static IStringLocalizer<SharedResource> CreateLocalizer()
        {
            var localizer = Substitute.For<IStringLocalizer<SharedResource>>();
            localizer[Arg.Any<string>()].Returns(ci =>
                new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
            return localizer;
        }

        // ── CreateCustomer ────────────────────────────────────────────────

        [Fact]
        public void CreateCustomer_ShouldReturnError_WhenNameIsNull()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer(null!, "01000000000", "system", addresses, localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Name", result.TopError.Code);
        }

        [Fact]
        public void CreateCustomer_ShouldReturnError_WhenNameIsEmpty()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("", "01000000000", "system", addresses, localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Name", result.TopError.Code);
        }

        [Fact]
        public void CreateCustomer_ShouldReturnError_WhenNameIsWhiteSpace()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("   ", "01000000000", "system", addresses, localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Name", result.TopError.Code);
        }

        [Fact]
        public void CreateCustomer_ShouldReturnError_WhenMobileIsNull()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("Ahmed", null!, "system", addresses, localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Mobile", result.TopError.Code);
        }

        [Fact]
        public void CreateCustomer_ShouldReturnError_WhenMobileIsEmpty()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("Ahmed", "", "system", addresses, localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Mobile", result.TopError.Code);
        }

        [Fact]
        public void CreateCustomer_ShouldReturnError_WhenMobileIsWhiteSpace()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("Ahmed", "   ", "system", addresses, localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("Invalide_Mobile", result.TopError.Code);
        }

        [Fact]
        public void CreateCustomer_ShouldSucceed_WithValidData()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("Ahmed", "01000000000", "system", addresses, localizer);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Ahmed", result.Value.Name);
            Assert.Equal("01000000000", result.Value.Mobile);
        }

        [Fact]
        public void CreateCustomer_ShouldAssignNewId_OnSuccess()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("Ahmed", "01000000000", "system", addresses, localizer);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value.Id);
        }

        [Fact]
        public void CreateCustomer_ShouldSetIsDeletedFalse_ByDefault()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("Ahmed", "01000000000", "system", addresses, localizer);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.IsDeleted);
        }

        [Fact]
        public void CreateCustomer_ShouldCreateWithAddresses_WhenAddressesProvided()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>
            {
                (AdressType.Home, "Cairo"),
                (AdressType.Work, "Giza")
            };

            var result = Customer.CreateCustomer("Ahmed", "01000000000", "system", addresses, localizer);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Addresses.Count);
        }

        [Fact]
        public void CreateCustomer_ShouldReturnError_WhenDuplicateAddressTypeInInitialList()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>
            {
                (AdressType.Home, "Cairo"),
                (AdressType.Home, "Alexandria")
            };

            var result = Customer.CreateCustomer("Ahmed", "01000000000", "system", addresses, localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("DuplicateAddressType", result.TopError.Code);
        }

        [Fact]
        public void CreateCustomer_ShouldSetCreatedByAndUpdatedBy()
        {
            var localizer = CreateLocalizer();
            var addresses = new List<(AdressType, string)>();

            var result = Customer.CreateCustomer("Ahmed", "01000000000", "admin", addresses, localizer);

            Assert.True(result.IsSuccess);
            Assert.Equal("admin", result.Value.CreatedBy);
            Assert.Equal("admin", result.Value.UpdatedBy);
        }

        // ── UpdateCustomer ────────────────────────────────────────────────

        [Fact]
        public void UpdateCustomer_ShouldUpdateName_WhenNewNameProvided()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            customer.UpdateCustomer("Mohamed", null, "admin");

            Assert.Equal("Mohamed", customer.Name);
        }

        [Fact]
        public void UpdateCustomer_ShouldUpdateMobile_WhenNewMobileProvided()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            customer.UpdateCustomer(null, "01111111111", "admin");

            Assert.Equal("01111111111", customer.Mobile);
        }

        [Fact]
        public void UpdateCustomer_ShouldNotChangeName_WhenNullNameProvided()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            customer.UpdateCustomer(null, null, "admin");

            Assert.Equal("Ahmed", customer.Name);
        }

        [Fact]
        public void UpdateCustomer_ShouldSetUpdatedBy()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            customer.UpdateCustomer("New Name", null, "admin");

            Assert.Equal("admin", customer.UpdatedBy);
        }

        // ── AddAddress ────────────────────────────────────────────────────

        [Fact]
        public void AddAddress_ShouldSucceed_WhenAddressTypeIsUnique()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            var result = customer.AddAddress(AdressType.Home, "Cairo", localizer);

            Assert.True(result.IsSuccess);
            Assert.Single(customer.Addresses);
        }

        [Fact]
        public void AddAddress_ShouldReturnError_WhenDuplicateAddressType()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;
            customer.AddAddress(AdressType.Home, "Cairo", localizer);

            var result = customer.AddAddress(AdressType.Home, "Giza", localizer);

            Assert.False(result.IsSuccess);
            Assert.Equal("DuplicateAddressType", result.TopError.Code);
        }

        [Fact]
        public void AddAddress_ShouldAllowDifferentTypes_OnSameCustomer()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            var r1 = customer.AddAddress(AdressType.Home, "Cairo", localizer);
            var r2 = customer.AddAddress(AdressType.Work, "Giza", localizer);
            var r3 = customer.AddAddress(AdressType.Other, "Alex", localizer);

            Assert.True(r1.IsSuccess);
            Assert.True(r2.IsSuccess);
            Assert.True(r3.IsSuccess);
            Assert.Equal(3, customer.Addresses.Count);
        }

        [Fact]
        public void AddAddress_ShouldReturnError_WhenValueIsEmpty()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            var result = customer.AddAddress(AdressType.Home, "", localizer);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void AddAddress_ShouldLinkAddressToCustomer()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            customer.AddAddress(AdressType.Home, "Cairo", localizer);
            var address = customer.Addresses.First();

            Assert.Equal(customer.Id, address.CustomerId);
        }

        // ── DeleteCustomer ────────────────────────────────────────────────

        [Fact]
        public void DeleteCustomer_ShouldSetIsDeletedTrue()
        {
            var localizer = CreateLocalizer();
            var customer = Customer.CreateCustomer("Ahmed", "01000000000", "system", [], localizer).Value;

            customer.DeleteCustomer();

            Assert.True(customer.IsDeleted);
        }
    }
}
