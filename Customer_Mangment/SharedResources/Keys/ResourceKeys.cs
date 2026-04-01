namespace Customer_Mangment.SharedResources.Keys
{
    public static class ResourceKeys
    {
        //   Auth     
        public static class Auth
        {
            public const string LoginRequired = "Auth.Login.Required";
            public const string TokenExpired = "Auth.Token.Expired";
            public const string RefreshTokenExpired = "Auth.RefreshToken.Expired";
            public const string UserIdClaimInvalid = "Auth.UserIdClaim.Invalid";
            public const string Unauthorized = "Auth.Unauthorized";
        }

        //   User     
        public static class User
        {
            public const string NotFound = "User.NotFound";
            public const string IdRequired = "User.Id.Required";
        }

        //   Customer   
        public static class Customer
        {
            public const string NotFound = "Customer.NotFound";
            public const string AlreadyExists = "Customer.AlreadyExists";
            public const string IdRequired = "Customer.Id.Required";
            public const string HistoryNotFound = "Customer.History.NotFound";
            public const string Deleted = "Customer.Deleted";
            public const string Updated = "Customer.Updated";
            public const string Created = "Customer.Created";
        }

        //   Address    
        public static class Address
        {
            public const string NotFound = "Address.NotFound";
            public const string Duplicate = "Address.Duplicate";
            public const string HistoryNotFound = "Address.History.NotFound";
            public const string IdRequired = "Address.Id.Required";
            public const string Deleted = "Address.Deleted";
            public const string Updated = "Address.Updated";
            public const string Created = "Address.Created";
        }

        //   Validation  
        public static class Validation
        {
            public const string NameRequired = "Validation.Name.Required";
            public const string NameEmpty = "Validation.Name.Empty";
            public const string MobileRequired = "Validation.Mobile.Required";
            public const string MobileInvalid = "Validation.Mobile.Invalid";
            public const string EmailRequired = "Validation.Email.Required";
            public const string PasswordRequired = "Validation.Password.Required";
            public const string AddressTypeInvalid = "Validation.AddressType.Invalid";
            public const string AddressValueRequired = "Validation.AddressValue.Required";
            public const string AddressValueEmpty = "Validation.AddressValue.Empty";
        }

        //   General    
        public static class General
        {
            public const string InternalServerError = "General.InternalServerError";
            public const string NotFound = "General.NotFound";
            public const string Conflict = "General.Conflict";
            public const string ValidationErrors = "General.ValidationErrors";
            public const string Success = "General.Success";
        }

        //   Token     
        public static class Token
        {
            public const string GenerationFailed = "Token.Generation.Failed";
            public const string IdRequired = "Token.Id.Required";
            public const string TokenRequired = "Token.Token.Required";
            public const string UserIdRequired = "Token.UserId.Required";
            public const string ExpiryInvalid = "Token.Expiry.Invalid";
        }

        //   Migration   
        public static class Migration
        {
            public const string Skipped = "Migration.Skipped";
            public const string Failed = "Migration.Failed";
        }
    }
}
