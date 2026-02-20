using FluentValidation;
namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.DeleteAddress
{
    public sealed class DeleteAddressValidator : AbstractValidator<DeleteAddressCommand>
    {
        public DeleteAddressValidator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("You have to be login first");
            RuleFor(c => c.AddressId)
                .NotEmpty().WithMessage("AddressId is required.");
        }
    };
}
