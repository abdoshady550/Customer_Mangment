using Customer_Mangment.SharedResources;
using Customer_Mangment.SharedResources.Keys;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.CQRS.Identity.Commands.GenerateTokens;

public sealed class GenerateTokenComandValidator : AbstractValidator<GenerateTokenComand>
{
    public GenerateTokenComandValidator(IStringLocalizer<SharedResource> l)
    {
        RuleFor(r => r.Email)
            .NotNull().NotEmpty()
            .WithErrorCode("Email_Null_Or_Empty")
            .WithMessage(_ => l[ResourceKeys.Validation.EmailRequired]);

        RuleFor(r => r.Password)
            .NotNull().NotEmpty()
            .WithErrorCode("Password_Null_Or_Empty")
            .WithMessage(_ => l[ResourceKeys.Validation.PasswordRequired]); ;
    }
}