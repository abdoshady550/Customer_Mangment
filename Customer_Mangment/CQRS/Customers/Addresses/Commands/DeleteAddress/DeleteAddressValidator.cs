using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using FluentValidation;
using Microsoft.Extensions.Localization;
namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress
{
    public sealed class DeleteAddressValidator : AbstractValidator<DeleteAddressCommand>
    {
        public DeleteAddressValidator(IStringLocalizer<SharedResource> l)
        {
            RuleFor(c => c.UserId)
                            .NotEmpty()
                            .WithMessage(_ => l[ResourceKeys.Auth.LoginRequired]);

            RuleFor(c => c.AddressId)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Address.IdRequired]);
        }
    };
}
