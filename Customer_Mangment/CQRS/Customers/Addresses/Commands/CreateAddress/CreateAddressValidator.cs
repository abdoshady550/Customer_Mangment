using Customer_Mangment.Model.Entities;
using FluentValidation;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.CreateAddress
{
    public sealed class CreateAddressValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressValidator()
        {

            RuleFor(x => x.Type).NotEmpty().Must(x => x.GetType() == typeof(AdressType));

            RuleFor(x => x.Value).NotEmpty();

        }

    };
}
