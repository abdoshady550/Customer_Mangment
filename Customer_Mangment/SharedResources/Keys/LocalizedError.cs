using Microsoft.Extensions.Localization;

namespace Customer_Mangment.SharedResources.Keys
{
    public static class LocalizedError
    {
        //   NotFound  

        public static Model.Results.Error NotFound(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Model.Results.Error.NotFound(code, Format(l, resourceKey, args));

        //   Conflict  

        public static Model.Results.Error Conflict(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Model.Results.Error.Conflict(code, Format(l, resourceKey, args));

        //   Unauthorized  

        public static Model.Results.Error Unauthorized(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Model.Results.Error.Unauthorized(code, Format(l, resourceKey, args));

        //   Validation   

        public static Model.Results.Error Validation(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Model.Results.Error.Validation(code, Format(l, resourceKey, args));

        //   Failure   

        public static Model.Results.Error Failure(
            IStringLocalizer<SharedResource> l,
            string code,
            string resourceKey,
            params object[] args)
            => Model.Results.Error.Failure(code, Format(l, resourceKey, args));

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
