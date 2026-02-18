using Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress;
using FluentValidation;

namespace Customer_Mangment.CQRS.Customers.Commands.CreateCustomer
{
    public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerValidator()
        {
            RuleFor(c => c.userId).NotEmpty().WithMessage("You have to be login first");
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required");
            RuleFor(x => x.Mobile)
                .NotEmpty().WithMessage("Mobile number is required")
                .Matches(@"^01[0-9]{9}$").WithMessage("Mobile number is not valid");

            RuleForEach(x => x.Adresses).SetValidator(new CreateAddressValidator());
        }
    }
}
