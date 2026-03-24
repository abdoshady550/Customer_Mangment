using Quartz;

namespace Customer_Mangment.Extensions;

public static class QuartzExtensions
{
    public static IServiceCollection AddQuartzJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddQuartz(q =>
        {
            q.SchedulerId = "AUTO";
            q.SchedulerName = "CustomerMgmtScheduler";

            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 5;
            });

            q.UsePersistentStore(store =>
            {
                store.UseProperties = true;
                store.RetryInterval = TimeSpan.FromMinutes(1);

                store.UseSqlServer(sql =>
                {
                    sql.ConnectionString = configuration
                        .GetConnectionString("DefaultConnection")!;

                    sql.TablePrefix = "QRTZ_";
                });

                store.UseNewtonsoftJsonSerializer();
            });

            var jobKey = new JobKey("MigrationJob");

            q.AddJob<MigrationJob>(opts => opts
                .WithIdentity(jobKey)
                .StoreDurably());

            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("MigrationJob-trigger", "MigrationGroup")
                .WithCronSchedule("0 * * * * ?")
                .StartNow());
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
            options.AwaitApplicationStarted = true;
        });

        return services;
    }
}