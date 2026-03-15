using FluentValidation;

namespace Customer_Mangment.CQRS.Customers.Queries.Report
{
    public sealed class GetCustomerReportValidator : AbstractValidator<GetCustomerReportQuery>
    {
        public GetCustomerReportValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("You must be logged in first.");
            RuleFor(x => x.From)
                .NotEmpty()
                .WithMessage("From field is requred");
            RuleFor(x => x.To)
                .NotEmpty()
                .WithMessage("From field is requred");
            When(x => x.From.HasValue && x.To.HasValue, () =>
            {
                RuleFor(x => x.To)
                    .GreaterThanOrEqualTo(x => x.From)
                    .WithMessage("'To' must be greater than or equal to 'From'.");
            });
        }
    }
}
