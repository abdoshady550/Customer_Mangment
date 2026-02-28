using FluentValidation;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress
{
    public sealed class AddAddressValidator : AbstractValidator<AddAddressCommand>
    {
        public AddAddressValidator()
        {

            RuleFor(x => x.Type).IsInEnum();
            RuleFor(x => x.Value).NotEmpty();

        }

    };
}
