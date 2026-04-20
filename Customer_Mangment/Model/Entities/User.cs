using Microsoft.AspNetCore.Identity;

namespace Customer_Mangment.Model.Entities
{
    public class User : IdentityUser;


    public enum Role
    {
        User = 0,
        Admin = 1
    }

}
