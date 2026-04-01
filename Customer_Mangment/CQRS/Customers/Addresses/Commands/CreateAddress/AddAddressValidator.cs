using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress
{
    public sealed class CreateAddressValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressValidator(IStringLocalizer<SharedResource> l)
        {

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage(_ => l[ResourceKeys.Validation.AddressTypeInvalid]);

            RuleFor(x => x.Value)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Validation.AddressValueRequired]);

        }

    };
}
