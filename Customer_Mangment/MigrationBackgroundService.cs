using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;

namespace Customer_Mangment
{
    public sealed class MigrationBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<MigrationBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            var flags = configuration
                .GetSection(FeatureFlags.SectionName)
                .Get<FeatureFlags>() ?? new FeatureFlags();
            if (!flags.RunMigrationOnStartup)
            {
                logger.LogInformation("MigrationBackgroundService: skipped (RunMigrationOnStartup = false)");
                return;
            }

            var direction = flags.UseMongoDb ? "SQL To  MongoDB" : "MongoDB To  SQL";

            logger.LogInformation(
                "MigrationBackgroundService: starting ({Direction})", direction);

            using var scope = scopeFactory.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();

            try
            {
                MigrationResult result = flags.UseMongoDb
                    ? await migrationService.MigrateSqlToMongoAsync(ct)
                    : await migrationService.MigrateMongoToSqlAsync(ct);

                if (result.Success)
                {
                    logger.LogInformation(
                        "Migration completed successfully." +
                        "Direction={Direction} Customers={C} Addresses={A} Tokens={T} Users={U}",
                        result.Direction,
                        result.CustomersProcessed,
                        result.AddressesProcessed,
                        result.RefreshTokensProcessed,
                        result.UsersProcessed);
                }
                else
                {
                    logger.LogWarning(
                        "Migration finished with {ErrorCount} error(s): {Errors}",
                        result.Errors.Count,
                        string.Join(" | ", result.Errors));
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Migration was cancelled.");
            }
            catch (Exception ex)
            {
                // Log but do NOT crash the app — migration is best-effort at startup
                logger.LogError(ex, "Migration background service encountered a fatal error.");
            }
        }
    }

}
