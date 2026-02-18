using FluentValidation;

namespace Customer_Mangment.CQRS.Customers.Commands.DeleteCustomer
{
    public sealed class DeleteCustomerValidator : AbstractValidator<DeleteCustomerCommand>
    {
        public DeleteCustomerValidator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("You have to be login first");
            RuleFor(c => c.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.");
        }
    };
}
