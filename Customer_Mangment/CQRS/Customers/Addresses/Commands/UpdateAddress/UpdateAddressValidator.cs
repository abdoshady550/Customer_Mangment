using FluentValidation;

namespace Customer_Mangment.CQRS.Customers.Addresses.Commands.UpdateAddress
{
    public sealed class UpdateAddressValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressValidator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty()
                .WithMessage("You have to be login first");

            RuleFor(x => x.AddressId)
                .NotEmpty()
                .WithMessage("AddressId is required.");
            RuleFor(x => x.Type)
                .IsInEnum()
                .When(x => x.Type.HasValue)
                .WithMessage("Invalid address type.");

            RuleFor(x => x.Value)
                .Must(value => !string.IsNullOrEmpty(value))
                .When(x => x.Value != null)
                .WithMessage("Address value is required.");
        }
    };
}
