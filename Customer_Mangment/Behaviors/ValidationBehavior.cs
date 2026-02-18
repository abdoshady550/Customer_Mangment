using FluentValidation;
using MediatR;

namespace Customer_Mangment.Behaviors
{
    public class ValidationBehavior<TReq, TRes>(IEnumerable<IValidator<TReq>> validators)
        : IPipelineBehavior<TReq, TRes>
        where TReq : notnull
    {
        public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
                return await next(cancellationToken);

            var context = new ValidationContext<TReq>(request);

            var Results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = Results.SelectMany(r => r.Errors).Where(e => e != null).ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);

            return await next(cancellationToken);
        }
    }
}
