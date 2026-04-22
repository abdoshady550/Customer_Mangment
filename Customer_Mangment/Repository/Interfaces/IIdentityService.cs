using Customer_Mangment.CQRS.Identity.Dto;

namespace Customer_Mangment.Repository.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> IsInRoleAsync(string userId, string role);

        Task<bool> AuthorizeAsync(string userId, string? policyName);

        Task<Model.Results.Result<AppUserDto>> AuthenticateAsync(string email, string password);

        Task<Model.Results.Result<AppUserDto>> GetUserByIdAsync(string userId);

        Task<string?> GetUserNameAsync(string userId);
    }
}
