namespace Customer_Mangment.Model.Results
{
    public sealed class MigrationResult
    {
        public bool Success { get; init; }
        public string Direction { get; init; } = string.Empty;
        public int CustomersProcessed { get; init; }
        public int AddressesProcessed { get; init; }
        public int RefreshTokensProcessed { get; init; }
        public int UsersProcessed { get; init; }
        public List<string> Errors { get; init; } = [];
        public DateTime MigratedAt { get; init; } = DateTime.UtcNow;

        public static MigrationResult Ok(
            string direction, int customers, int addresses, int tokens, int users) => new()
            {
                Success = true,
                Direction = direction,
                CustomersProcessed = customers,
                AddressesProcessed = addresses,
                RefreshTokensProcessed = tokens,
                UsersProcessed = users,
            };

        public static MigrationResult Fail(string direction, List<string> errors) => new()
        {
            Success = false,
            Direction = direction,
            Errors = errors,
        };

        public static MigrationResult Skipped(string reason) => new()
        {
            Success = true,
            Direction = "Skipped",
            Errors = [reason],
        };
    }
}
