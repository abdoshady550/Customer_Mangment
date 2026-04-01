using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.Repository.Services;

public class IdentityService(UserManager<User> userManager,
                             IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
                             IStringLocalizer<SharedResource> localizer,
                             IAuthorizationService authorizationService) : IIdentityService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    private readonly IStringLocalizer<SharedResource> _localizer = localizer;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string? policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName!);

        return result.Succeeded;
    }

    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return LocalizedError.NotFound(_localizer, "User_Not_Found", ResourceKeys.User.NotFound, UtilityService.MaskEmail(email));

        }

        if (!user.EmailConfirmed)
        {
            return LocalizedError.Conflict(_localizer, "Email_Not_Confirmed", ResourceKeys.Validation.EmailRequired);
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return LocalizedError.Conflict(_localizer, "Password_Incorrect", ResourceKeys.Validation.PasswordRequired);

        }

        return new AppUserDto(user.Id, user.Email!, await _userManager.GetRolesAsync(user), await _userManager.GetClaimsAsync(user));
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException(nameof(userId));

        var roles = await _userManager.GetRolesAsync(user);

        var claims = await _userManager.GetClaimsAsync(user);

        return new AppUserDto(user.Id, user.Email!, roles, claims);
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }
}