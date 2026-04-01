using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.UpdateAddress
{
    public sealed class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressValidator(IStringLocalizer<SharedResource> l)
        {
            RuleFor(c => c.UserId)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Auth.LoginRequired]);

            RuleFor(x => x.AddressId)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Address.IdRequired]);

            RuleFor(x => x.Type)
                .IsInEnum()
                .When(x => x.Type.HasValue)
                .WithMessage(_ => l[ResourceKeys.Validation.AddressTypeInvalid]);

            RuleFor(x => x.Value)
                .Must(v => !string.IsNullOrEmpty(v))
                .When(x => x.Value is not null)
                .WithMessage(_ => l[ResourceKeys.Validation.AddressValueEmpty]);
        }
    };
}
