namespace Customer_Mangment.MultiTenancy;

public sealed class TenantContext : ITenantContext
{
    public string TenantId { get; private set; } = string.Empty;
    public string ConnectionString { get; private set; } = string.Empty;
    public bool IsResolved { get; private set; }

    internal void Resolve(string tenantId, string connectionString)
    {
        TenantId = tenantId;
        ConnectionString = connectionString;
        IsResolved = true;
    }
}
