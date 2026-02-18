using FluentValidation;

namespace Customer_Mangment.CQRS.Customers.Commands.UpdateCustomer
{
    public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
    {
        public UpdateCustomerValidator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("You have to be login first");

            RuleFor(c => c.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.");

            RuleFor(x => x.Name)
                    .Must(name => !string.IsNullOrWhiteSpace(name))
                    .When(x => x.Name != null).WithMessage("Name cannot be empty");

            RuleFor(x => x.Mobile)
                .Matches(@"^01[0-9]{9}$")
                .When(x => x.Mobile != null)
                .WithMessage("Mobile number is not valid");
        }

    };
}
