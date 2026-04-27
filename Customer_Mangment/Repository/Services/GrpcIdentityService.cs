using Customer_Mangment.Contracts.Grpc;
using Customer_Mangment.CQRS.Identity.Dto;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.Repository.Services;

public sealed class GrpcIdentityService(
    IdentityGrpcService.IdentityGrpcServiceClient grpcClient,
    IStringLocalizer<SharedResource> localizer) : IIdentityService
{
    public async Task<Model.Results.Result<AppUserDto>> AuthenticateAsync(
        string email, string password)
    {
        var response = await grpcClient.ValidateUserAsync(
            new ValidateUserRequest { Email = email, Password = password });

        if (!response.Success)
            return LocalizedError.Conflict(localizer,
                "Password_Incorrect", ResourceKeys.Validation.PasswordRequired);

        return new AppUserDto(response.UserId, response.Email,
            response.Roles.ToList(), []);
    }

    public async Task<Model.Results.Result<AppUserDto>> GetUserByIdAsync(string userId)
    {
        var response = await grpcClient.GetUserByIdAsync(
            new GetUserByIdRequest { UserId = userId });

        if (!response.Found)
            return LocalizedError.NotFound(localizer,
                "User_Not_Found", ResourceKeys.User.NotFound, userId);

        return new AppUserDto(response.UserId, response.Email,
            response.Roles.ToList(), []);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var response = await grpcClient.GetUserByIdAsync(
            new GetUserByIdRequest { UserId = userId });

        return response.Found &&
               response.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public Task<bool> AuthorizeAsync(string userId, string? policyName)
        => Task.FromResult(true);

    public Task<string?> GetUserNameAsync(string userId)
        => Task.FromResult<string?>(null);
}