using Customer_Mangment.Model.Entities;

namespace Customer_Mangment.Test.Domain
{
    public class CustomerHistoryTest
    {
        [Fact]
        public void CreateCustomerHistory_ShouldReturnError_WhenCustomerIdIsEmpty()
        {
            var customerId = Guid.Empty;

            var customerHistoryResult = CustomerHistory.CreateCustomerHistory(customerId,
                                                                              "Ahmed",
                                                                              "01000000000",
                                                                              "admin");
            Assert.False(customerHistoryResult.IsSuccess);
            Assert.Equal("Invalide_Customer", customerHistoryResult.TopError.Code);
        }
        [Fact]
        public void CreateCustomerHistory_ShouldReturnError_WhenCustomerNameIsNull()
        {
            var customerId = Guid.NewGuid();
            string firstName = null;
            string secoundName = "";
            var firstCustomerHistoryResult = CustomerHistory.CreateCustomerHistory(customerId,
                                                                                   null,
                                                                                   "01000000000",
                                                                                   "admin");

            var secoundCustomerHistoryResult = CustomerHistory.CreateCustomerHistory(customerId,
                                                                                   secoundName,
                                                                                   "01000000000",
                                                                                   "admin");
            Assert.False(firstCustomerHistoryResult.IsSuccess);
            Assert.False(secoundCustomerHistoryResult.IsSuccess);

            Assert.Equal("Invalide_Customer_Name", firstCustomerHistoryResult.TopError.Code);
        }
        [Fact]
        public void CreateCustomerHistory_ShouldRetunError_WhenMobileIsNullOrWhiteSpace()
        {
            var customerId = Guid.NewGuid();

            string firstMobile = null;
            string secoundMobile = "";

            var firstCustomerHistoryResult = CustomerHistory.CreateCustomerHistory(customerId,
                                                                          "null",
                                                                          firstMobile,
                                                                          "admin");
            var secoundCustomerHistoryResult = CustomerHistory.CreateCustomerHistory(customerId,
                                                                          "null",
                                                                          secoundMobile,
                                                                          "admin");

            Assert.False(firstCustomerHistoryResult.IsSuccess);
            Assert.False(secoundCustomerHistoryResult.IsSuccess);
            Assert.Equal("Invalide_Customer_Mobile", secoundCustomerHistoryResult.TopError.Code);
        }
        [Fact]
        public void UpdateCustomerHistory_ShouldReturnError_WhenCustomerIdIsEmpty()
        {
            var customerId = Guid.Empty;
            var createdAt = DateTime.UtcNow;

            var customerHistoryResult = CustomerHistory.UpdateCustomerHistory(customerId,
                                                                              "Ahmed",
                                                                              "01000000000",
                                                                              createdAt,
                                                                              "admin",
                                                                              "sasa",
                                                                              "sasa");
            Assert.False(customerHistoryResult.IsSuccess);
            Assert.Equal("Invalide_Customer", customerHistoryResult.TopError.Code);
        }

    }
}
