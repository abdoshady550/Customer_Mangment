using Customer_Mangment;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Customer_Mangment_Integrate.Test
{

    public class LocalizationTests : IClassFixture<WebApplicationFactory<IAssmblyMarker>>
    {
        private readonly WebApplicationFactory<IAssmblyMarker> _factory;
        private const string DefaultTenantId = "demo";


        // en
        private const string En_EmailRequired = "Email cannot be null or empty";
        private const string En_PasswordRequired = "Password cannot be null or empty";
        private const string En_NameRequired = "Name is required";
        private const string En_MobileRequired = "Mobile number is required";
        private const string En_MobileInvalid = "Mobile number is not valid";
        private const string En_AddressValue = "Address value is required";
        private const string En_AddressType = "Invalid address type";
        private const string En_CustomerIdRequired = "CustomerId is required";

        // ar
        private const string Ar_EmailRequired = "البريد الإلكتروني مطلوب";
        private const string Ar_PasswordRequired = "كلمة المرور مطلوبة";
        private const string Ar_NameRequired = "الاسم مطلوب";
        private const string Ar_MobileRequired = "رقم الهاتف مطلوب";
        private const string Ar_MobileInvalid = "رقم الهاتف غير صالح";
        private const string Ar_AddressValue = "قيمة العنوان مطلوبة";
        private const string Ar_AddressType = "نوع العنوان غير صالح";
        private const string Ar_CustomerIdRequired = "معرف العميل مطلوب";

        public LocalizationTests(WebApplicationFactory<IAssmblyMarker> factory)
            => _factory = factory;

        // ── HttpClient factories 

        private HttpClient AuthClient(string? acceptLanguage = null)
        {
            var http = _factory.CreateClient();
            if (!string.IsNullOrEmpty(acceptLanguage))
                http.DefaultRequestHeaders.AcceptLanguage.ParseAdd(acceptLanguage);
            return http;
        }

        private HttpClient TenantClient(string? acceptLanguage = null, string? bearerToken = null)
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId);
            if (!string.IsNullOrEmpty(acceptLanguage))
                http.DefaultRequestHeaders.AcceptLanguage.ParseAdd(acceptLanguage);
            if (!string.IsNullOrEmpty(bearerToken))
                http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", bearerToken);
            return http;
        }


        private static StringContent Json(object payload)
            => new(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        private static string WithLang(string path, string lang)
            => path.Contains('?') ? $"{path}&lang={lang}" : $"{path}?lang={lang}";

        private async Task<string> PostBodyAsync(HttpClient http, string path, object payload)
        {
            var resp = await http.PostAsync(path, Json(payload));
            return await resp.Content.ReadAsStringAsync();
        }

        private async Task<string> PutBodyAsync(HttpClient http, string path, object payload)
        {
            var resp = await http.PutAsync(path, Json(payload));
            return await resp.Content.ReadAsStringAsync();
        }

        // ── Token  

        private async Task<string> GetAdminTokenAsync()
        {
            var http = _factory.CreateClient();
            http.DefaultRequestHeaders.Add("X-Tenant-Id", DefaultTenantId);
            var resp = await http.PostAsync(
                "/api/Auth/token/generate",
                Json(new { email = "admin@test.com", password = "Admin@123" }));
            var body = await resp.Content.ReadAsStringAsync();
            dynamic obj = JsonConvert.DeserializeObject(body)!;
            return (string)obj.accessToken;
        }

        // Auth – empty email

        [Fact]
        public async Task Auth_EmptyEmail_Header_En_ReturnsEnglishError()
        {
            var body = await PostBodyAsync(
                AuthClient("en"),
                "/api/Auth/token/generate",
                new { email = "", password = "Admin@123" });

            Assert.Contains(En_EmailRequired, body);
        }

        [Fact]
        public async Task Auth_EmptyEmail_Header_Ar_ReturnsArabicError()
        {
            var body = await PostBodyAsync(
                AuthClient("ar"),
                "/api/Auth/token/generate",
                new { email = "", password = "Admin@123" });

            Assert.Contains(Ar_EmailRequired, body);
        }

        [Fact]
        public async Task Auth_EmptyEmail_Header_ArEG_ReturnsArabicError()
        {
            var body = await PostBodyAsync(
                AuthClient("ar-EG"),
                "/api/Auth/token/generate",
                new { email = "", password = "Admin@123" });

            Assert.Contains(Ar_EmailRequired, body);
        }

        [Fact]
        public async Task Auth_EmptyEmail_Header_EnUS_ReturnsEnglishError()
        {
            var body = await PostBodyAsync(
                AuthClient("en-US"),
                "/api/Auth/token/generate",
                new { email = "", password = "Admin@123" });

            Assert.Contains(En_EmailRequired, body);
        }

        // ?lang= query param
        [Fact]
        public async Task Auth_EmptyEmail_LangParam_En_ReturnsEnglishError()
        {
            var body = await PostBodyAsync(
                AuthClient(),
                WithLang("/api/Auth/token/generate", "en"),
                new { email = "", password = "Admin@123" });

            Assert.Contains(En_EmailRequired, body);
        }

        [Fact]
        public async Task Auth_EmptyEmail_LangParam_Ar_ReturnsArabicError()
        {
            var body = await PostBodyAsync(
                AuthClient(),
                WithLang("/api/Auth/token/generate", "ar"),
                new { email = "", password = "Admin@123" });

            Assert.Contains(Ar_EmailRequired, body);
        }

        [Fact]
        public async Task Auth_LangParam_En_WinsOver_Header_Ar()
        {
            var body = await PostBodyAsync(
                AuthClient("ar"),
                WithLang("/api/Auth/token/generate", "en"),
                new { email = "", password = "Admin@123" });

            Assert.Contains(En_EmailRequired, body);
            Assert.DoesNotContain(Ar_EmailRequired, body);
        }

        [Fact]
        public async Task Auth_LangParam_Ar_WinsOver_Header_En()
        {
            var body = await PostBodyAsync(
                AuthClient("en"),
                WithLang("/api/Auth/token/generate", "ar"),
                new { email = "", password = "Admin@123" });

            Assert.Contains(Ar_EmailRequired, body);
            Assert.DoesNotContain(En_EmailRequired, body);
        }

        // Fallback
        [Fact]
        public async Task Auth_UnsupportedLang_FallsBackToEnglish()
        {
            var body = await PostBodyAsync(
                AuthClient(),
                WithLang("/api/Auth/token/generate", "fr"),
                new { email = "", password = "Admin@123" });

            Assert.Contains(En_EmailRequired, body);
        }

        [Fact]
        public async Task Auth_NoLangNoHeader_DefaultsToEnglish()
        {
            var body = await PostBodyAsync(
                AuthClient(),
                "/api/Auth/token/generate",
                new { email = "", password = "Admin@123" });

            Assert.Contains(En_EmailRequired, body);
        }

        // Auth – empty password

        [Fact]
        public async Task Auth_EmptyPassword_En_ReturnsEnglishError()
        {
            var body = await PostBodyAsync(
                AuthClient("en"),
                "/api/Auth/token/generate",
                new { email = "admin@test.com", password = "" });

            Assert.Contains(En_PasswordRequired, body);
        }

        [Fact]
        public async Task Auth_EmptyPassword_Ar_ReturnsArabicError()
        {
            var body = await PostBodyAsync(
                AuthClient("ar"),
                "/api/Auth/token/generate",
                new { email = "admin@test.com", password = "" });

            Assert.Contains(Ar_PasswordRequired, body);
        }

        // =================================================================
        // Auth – both fields empty
        // =================================================================

        [Fact]
        public async Task Auth_BothEmpty_En_ReturnsBothEnglishErrors()
        {
            var body = await PostBodyAsync(
                AuthClient("en"),
                "/api/Auth/token/generate",
                new { email = "", password = "" });

            Assert.Contains(En_EmailRequired, body);
            Assert.Contains(En_PasswordRequired, body);
        }

        [Fact]
        public async Task Auth_BothEmpty_Ar_ReturnsBothArabicErrors()
        {
            var body = await PostBodyAsync(
                AuthClient("ar"),
                "/api/Auth/token/generate",
                new { email = "", password = "" });

            Assert.Contains(Ar_EmailRequired, body);
            Assert.Contains(Ar_PasswordRequired, body);
        }

        // Smoke: same payload → different bodies

        [Fact]
        public async Task SameRequest_En_And_Ar_ReturnDifferentBodies()
        {
            var payload = new { email = "", password = "" };
            var en = await PostBodyAsync(AuthClient("en"), "/api/Auth/token/generate", payload);
            var ar = await PostBodyAsync(AuthClient("ar"), "/api/Auth/token/generate", payload);

            Assert.NotEqual(en, ar);
            Assert.Contains(En_EmailRequired, en);
            Assert.Contains(Ar_EmailRequired, ar);
        }

        // CreateCustomer – name

        [Fact]
        public async Task CreateCustomer_EmptyName_En_ReturnsEnglishError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("en", token),
                "/api/Customer/add",
                new { name = "", mobile = "01000000000", adresses = Array.Empty<object>() });

            Assert.Contains(En_NameRequired, body);
        }

        [Fact]
        public async Task CreateCustomer_EmptyName_Ar_ReturnsArabicError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("ar", token),
                "/api/Customer/add",
                new { name = "", mobile = "01000000000", adresses = Array.Empty<object>() });

            Assert.Contains(Ar_NameRequired, body);
        }

        // CreateCustomer – mobile

        [Fact]
        public async Task CreateCustomer_EmptyMobile_En_ReturnsEnglishError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("en", token),
                "/api/Customer/add",
                new { name = "Ahmed", mobile = "", adresses = Array.Empty<object>() });

            Assert.Contains(En_MobileRequired, body);
        }

        [Fact]
        public async Task CreateCustomer_EmptyMobile_Ar_ReturnsArabicError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("ar", token),
                "/api/Customer/add",
                new { name = "Ahmed", mobile = "", adresses = Array.Empty<object>() });

            Assert.Contains(Ar_MobileRequired, body);
        }

        [Fact]
        public async Task CreateCustomer_InvalidMobile_En_ReturnsEnglishError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("en", token),
                "/api/Customer/add",
                new { name = "Ahmed", mobile = "123", adresses = Array.Empty<object>() });

            Assert.Contains(En_MobileInvalid, body);
        }

        [Fact]
        public async Task CreateCustomer_InvalidMobile_Ar_ReturnsArabicError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("ar", token),
                "/api/Customer/add",
                new { name = "Ahmed", mobile = "123", adresses = Array.Empty<object>() });

            Assert.Contains(Ar_MobileInvalid, body);
        }

        [Fact]
        public async Task CreateCustomer_AllEmpty_En_ReturnsBothErrors()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("en", token),
                "/api/Customer/add",
                new { name = "", mobile = "", adresses = Array.Empty<object>() });

            Assert.Contains(En_NameRequired, body);
            Assert.Contains(En_MobileRequired, body);
        }

        [Fact]
        public async Task CreateCustomer_AllEmpty_Ar_ReturnsBothErrors()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("ar", token),
                "/api/Customer/add",
                new { name = "", mobile = "", adresses = Array.Empty<object>() });

            Assert.Contains(Ar_NameRequired, body);
            Assert.Contains(Ar_MobileRequired, body);
        }

        // =================================================================
        // CreateCustomer – nested address validator
        // =================================================================

        [Fact]
        public async Task CreateCustomer_EmptyAddressValue_En_ReturnsEnglishError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("en", token),
                "/api/Customer/add",
                new
                {
                    name = "Ahmed",
                    mobile = "01000000000",
                    adresses = new[] { new { type = 1, value = "" } }
                });

            Assert.Contains(En_AddressValue, body);
        }

        [Fact]
        public async Task CreateCustomer_EmptyAddressValue_Ar_ReturnsArabicError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("ar", token),
                "/api/Customer/add",
                new
                {
                    name = "Ahmed",
                    mobile = "01000000000",
                    adresses = new[] { new { type = 1, value = "" } }
                });

            Assert.Contains(Ar_AddressValue, body);
        }

        // =================================================================
        // UpdateCustomer – empty CustomerId
        // =================================================================

        [Fact]
        public async Task UpdateCustomer_EmptyCustomerId_En_ReturnsEnglishError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PutBodyAsync(
                TenantClient("en", token),
                $"/api/Customer/update?CustomerId={Guid.Empty}",
                new { name = "Test", mobile = "01000000000" });

            Assert.Contains(En_CustomerIdRequired, body);
        }

        [Fact]
        public async Task UpdateCustomer_EmptyCustomerId_Ar_ReturnsArabicError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PutBodyAsync(
                TenantClient("ar", token),
                $"/api/Customer/update?CustomerId={Guid.Empty}",
                new { name = "Test", mobile = "01000000000" });

            Assert.Contains(Ar_CustomerIdRequired, body);
        }

        // =================================================================
        // AddAddress – empty value / invalid type
        // =================================================================

        [Fact]
        public async Task AddAddress_EmptyValue_En_ReturnsEnglishError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("en", token),
                $"/api/CustomerAddress/add?CustomerId={Guid.NewGuid()}",
                new { type = 1, value = "" });

            Assert.Contains(En_AddressValue, body);
        }

        [Fact]
        public async Task AddAddress_EmptyValue_Ar_ReturnsArabicError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("ar", token),
                $"/api/CustomerAddress/add?CustomerId={Guid.NewGuid()}",
                new { type = 1, value = "" });

            Assert.Contains(Ar_AddressValue, body);
        }

        [Fact]
        public async Task AddAddress_InvalidType_En_ReturnsEnglishError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("en", token),
                $"/api/CustomerAddress/add?CustomerId={Guid.NewGuid()}",
                new { type = 99, value = "Cairo" });

            Assert.Contains(En_AddressType, body);
        }

        [Fact]
        public async Task AddAddress_InvalidType_Ar_ReturnsArabicError()
        {
            var token = await GetAdminTokenAsync();
            var body = await PostBodyAsync(
                TenantClient("ar", token),
                $"/api/CustomerAddress/add?CustomerId={Guid.NewGuid()}",
                new { type = 99, value = "Cairo" });

            Assert.Contains(Ar_AddressType, body);
        }
    }
}
