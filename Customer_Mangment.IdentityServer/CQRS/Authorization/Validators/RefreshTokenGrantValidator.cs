using Customer_Mangment.IdentityServer.CQRS.Authorization.Commands;
using FluentValidation;
using Localization.SharedResources;
using Localization.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization.Validators
{
    public sealed class RefreshTokenGrantValidator : AbstractValidator<RefreshTokenGrantCommand>
    {
        public RefreshTokenGrantValidator(IStringLocalizer<SharedResource> l)
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithMessage(_ => l[ResourceKeys.Auth.TokenExpired]);
        }
    }
}
