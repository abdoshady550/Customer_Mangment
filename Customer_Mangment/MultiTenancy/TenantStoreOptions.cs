namespace Customer_Mangment.MultiTenancy;

public sealed class TenantStoreOptions
{
    public const string SectionName = "Tenants";
    public Dictionary<string, TenantOptions> Tenants { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class TenantOptions
{
    public string? ConnectionString { get; set; }
}
