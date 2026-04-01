using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Commands.CreateCustomer
{
    public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerValidator(IStringLocalizer<SharedResource> l)
        {
            RuleFor(c => c.UserId)
                   .NotEmpty()
                   .WithMessage(_ => l[ResourceKeys.Auth.LoginRequired]);

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Validation.NameRequired]);

            RuleFor(x => x.Mobile)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Validation.MobileRequired])
                .Matches(@"^01[0-9]{9}$")
                .WithMessage(_ => l[ResourceKeys.Validation.MobileInvalid]);


            RuleForEach(x => x.Adresses).SetValidator(_ => new CreateAddressValidator(l));
        }
    }
}
