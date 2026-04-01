using Customer_Mangment.Model.Results;
using Microsoft.Extensions.Localization;

namespace Customer_Mangment.SharedResources.Keys
{
    public static class LocalizedError
    {
        //   NotFound  

        public static Error NotFound(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Error.NotFound(code, Format(l, resourceKey, args));

        //   Conflict  

        public static Error Conflict(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Error.Conflict(code, Format(l, resourceKey, args));

        //   Unauthorized  

        public static Error Unauthorized(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Error.Unauthorized(code, Format(l, resourceKey, args));

        //   Validation   

        public static Error Validation(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Error.Validation(code, Format(l, resourceKey, args));

        //   Failure   

        public static Error Failure(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Error.Failure(code, Format(l, resourceKey, args));

        //   private   

        private static string Format(
            IStringLocalizer<SharedResource> l,
            string resourceKey,
            object[] args)
        {
            var template = l[resourceKey];

            return args.Length == 0
                ? template.Value
                : string.Format(template.Value, args);
        }
    }
}
