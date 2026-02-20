using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model.Results;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> IsInRoleAsync(string userId, string role);

        Task<bool> AuthorizeAsync(string userId, string? policyName);

        Task<Result<AppUserDto>> AuthenticateAsync(string email, string password);

        Task<Result<AppUserDto>> GetUserByIdAsync(string userId);

        Task<string?> GetUserNameAsync(string userId);
    }
}
