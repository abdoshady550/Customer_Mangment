using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Customers.Commands.DeleteCustomer
{
    public sealed class DeleteCustomerValidator : AbstractValidator<DeleteCustomerCommand>
    {
        public DeleteCustomerValidator(IStringLocalizer<SharedResource> l)
        {
            RuleFor(c => c.UserId)
                  .NotEmpty()
                  .WithMessage(_ => l[ResourceKeys.Auth.LoginRequired]);

            RuleFor(c => c.CustomerId)
                .NotEmpty()
                .WithMessage(_ => l[ResourceKeys.Customer.IdRequired]);
        }
    };
}
