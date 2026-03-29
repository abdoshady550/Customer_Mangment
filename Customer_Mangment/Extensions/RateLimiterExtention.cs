using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Customer_Mangment.Extensions
{
    public static class RateLimiterExtention
    {
        public static IServiceCollection AddRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(option =>
            {
                //FixedWindow
                option.AddFixedWindowLimiter("DefaultPolicy", fixedOption =>
                {
                    fixedOption.Window = TimeSpan.FromMinutes(1);
                    fixedOption.PermitLimit = 10;
                    fixedOption.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    fixedOption.QueueLimit = 10;
                });
                //SlidingWindow
                option.AddSlidingWindowLimiter("SlidingPolicy", slideOtion =>
                {
                    slideOtion.Window = TimeSpan.FromMinutes(1);
                    slideOtion.PermitLimit = 10;
                    slideOtion.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    slideOtion.QueueLimit = 10;
                    slideOtion.SegmentsPerWindow = 6;
                    slideOtion.AutoReplenishment = true;
                });
                //Concurrency
                option.AddConcurrencyLimiter("ConncurrencyPolicy", conOtion =>
                {
                    conOtion.PermitLimit = 5;
                    conOtion.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    conOtion.QueueLimit = 5;

                });
                //User
                option.AddPolicy("UerPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(partitionKey: context.User.Identity?.Name ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(1),
                            PermitLimit = 10,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 10
                        }));
                //IP
                option.AddPolicy("IpPolicy", cotext =>
                RateLimitPartition.GetSlidingWindowLimiter(partitionKey: cotext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(1),
                    PermitLimit = 10,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10,
                    SegmentsPerWindow = 6,
                    AutoReplenishment = true,

                }));

                //Reject
                option.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            message = "Too many requests. Please wait before trying again.",
                            retryAfter = retryAfter.TotalSeconds
                        }, token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsJsonAsync(new
                        {
                            message = "Too many requests. Please try again later."
                        }, token);
                    }
                };
            });
            return services;
        }
    }
}
