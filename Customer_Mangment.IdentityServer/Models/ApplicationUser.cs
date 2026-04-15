using Microsoft.AspNetCore.Identity;

namespace Customer_Mangment.IdentityServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
    }
}
