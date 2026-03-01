namespace Customer_Mangment
{
    public sealed class FeatureFlags
    {
        public const string SectionName = "FeatureFlags";

        public bool UseMongoDb { get; set; } = true;
    }

}
