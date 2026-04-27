using Customer_Mangment.Contracts.Grpc;
using Customer_Mangment.IdentityServer.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;

namespace Customer_Mangment.IdentityServer.GrpcServices;

public sealed class IdentityGrpcServiceImpl(UserManager<ApplicationUser> userManager,
                                            SignInManager<ApplicationUser> signInManager,
                                            ILogger<IdentityGrpcServiceImpl> logger)
    : IdentityGrpcService.IdentityGrpcServiceBase
{
    public override async Task<UserResponse> GetUserById(
        GetUserByIdRequest request, ServerCallContext context)
    {
        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)
        {
            logger.LogWarning("gRPC GetUserById: user {Id} not found", request.UserId);
            return new UserResponse { Found = false };
        }

        var roles = await userManager.GetRolesAsync(user);

        return new UserResponse
        {
            Found = true,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = { roles }
        };
    }

    public override async Task<ValidateUserResponse> ValidateUser(
        ValidateUserRequest request, ServerCallContext context)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new ValidateUserResponse { Success = false };

        var result = await signInManager
            .CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
            return new ValidateUserResponse { Success = false };

        var roles = await userManager.GetRolesAsync(user);

        return new ValidateUserResponse
        {
            Success = true,
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Roles = { roles }
        };
    }
}