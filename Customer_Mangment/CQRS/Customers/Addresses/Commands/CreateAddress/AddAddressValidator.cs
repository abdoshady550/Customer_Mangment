using FluentValidation;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress
{
    public sealed class CreateAddressValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressValidator()
        {

            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.Value).NotEmpty();

        }

    };
}
