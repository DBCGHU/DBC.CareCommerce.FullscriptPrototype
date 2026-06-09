namespace DBC.CareCommerce.WindowsService
{
    public sealed class CareCommerceServiceSettings
    {
        public string SqlConnectionString { get; set; }

        public int FullscriptDispatchIntervalSeconds { get; set; }

        public CareCommerceServiceSettings()
        {
            FullscriptDispatchIntervalSeconds = 60;
        }
    }
}