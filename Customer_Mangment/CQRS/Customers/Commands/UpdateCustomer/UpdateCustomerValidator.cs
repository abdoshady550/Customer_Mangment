using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Commands.UpdateCustomer
{
    public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
    {
        public UpdateCustomerValidator(IStringLocalizer<SharedResource> l)
        {
            RuleFor(c => c.UserId)
                .NotEmpty()
                .WithErrorCode(ResourceKeys.User.IdRequired)
                .WithMessage(_ => l[ResourceKeys.Auth.LoginRequired]);

            RuleFor(c => c.CustomerId)
                .NotEmpty()
                .WithErrorCode(ResourceKeys.General.ValidationErrors)
                .WithMessage(_ => l[ResourceKeys.Customer.IdRequired]);

            RuleFor(x => x.Name)
                .Must(n => !string.IsNullOrWhiteSpace(n))
                .When(x => x.Name is not null)
                .WithErrorCode(ResourceKeys.General.ValidationErrors)

                .WithMessage(_ => l[ResourceKeys.Validation.NameEmpty]);

            RuleFor(x => x.Mobile)
                .Matches(@"^01[0-9]{9}$")
                .When(x => !string.IsNullOrEmpty(x.Mobile))
                .WithErrorCode(ResourceKeys.General.ValidationErrors)
                .WithMessage(_ => l[ResourceKeys.Validation.MobileInvalid]);
        }
    }
}