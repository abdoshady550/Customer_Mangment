using Customer_Mangment.IdentityServer.CQRS.Authorization.Commands;
using FluentValidation;
using Localization.SharedResources;
using Localization.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization.Validators
{
    public sealed class PasswordGrantValidator : AbstractValidator<PasswordGrantCommand>
    {
        public PasswordGrantValidator(IStringLocalizer<SharedResource> l)
        {
            RuleFor(x => x.Request.Username)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Validation.EmailRequired]);

            RuleFor(x => x.Request.Password)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Validation.PasswordRequired]);
        }
    }
}
