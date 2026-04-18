using Customer_Mangment.IdentityServer.CQRS.Authorization.Commands;
using FluentValidation;
using Localization.SharedResources;
using Localization.SharedResources.Keys;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.IdentityServer.CQRS.Authorization.Validators
{
    public sealed class ClientCredentialsGrantValidator : AbstractValidator<ClientCredentialsTokenCommand>
    {
        public ClientCredentialsGrantValidator(IStringLocalizer<SharedResource> l)
        {
            RuleFor(x => x.Request.ClientId)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Client.NotFound]);
        }
    }
}
