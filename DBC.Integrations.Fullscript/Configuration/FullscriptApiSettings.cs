namespace DBC.Integrations.Fullscript.Configuration
{
    public sealed class FullscriptApiSettings
    {
        public string ClientMode { get; set; } = "Stub";

        public string BaseUrl { get; set; }

        public string ApiToken { get; set; }

        public bool Enabled { get; set; }
    }
}