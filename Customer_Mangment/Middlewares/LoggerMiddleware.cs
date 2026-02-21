namespace Customer_Mangment.Middlewares
{
    public class LoggerMiddleware(ILogger<LoggerMiddleware> logger) : IMiddleware
    {
        private readonly ILogger<LoggerMiddleware> _logger = logger;

        private const string MaskedEndpoint = "/api/Auth/token/generate";

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path.Value ?? "";
            var isMasked = path.Equals(MaskedEndpoint, StringComparison.OrdinalIgnoreCase);

            // Request Body
            context.Request.EnableBuffering();
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Response Body
            var originalStream = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await next(context);

            memStream.Position = 0;
            var responseBody = await new StreamReader(memStream).ReadToEndAsync();
            memStream.Position = 0;
            await memStream.CopyToAsync(originalStream);
            context.Response.Body = originalStream;

            // Log
            logger.LogInformation("""
            ─ Request
            Method : {Method}
            Path   : {Path}
            Body   : {RequestBody}
            ─ Response 
            Status : {Status}
            Body   : {ResponseBody}
            ──
            """,
                context.Request.Method,
                path,
                isMasked ? "***MASKED***" : requestBody,
                context.Response.StatusCode,
                isMasked ? "***MASKED***" : responseBody);
        }


    }
}