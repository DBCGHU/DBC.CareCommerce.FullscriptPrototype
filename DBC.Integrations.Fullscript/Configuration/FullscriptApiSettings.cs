namespace DBC.Integrations.Fullscript.Configuration
{
    public sealed class FullscriptApiSettings
    {
        public string ClientMode { get; set; } = "Stub";

        public string BaseUrl { get; set; }

        public string ApiToken { get; set; }

        public bool Enabled { get; set; }

        public string OAuthClientId { get; set; }

        public string OAuthClientSecret { get; set; }

        public string OAuthRedirectUri { get; set; }

        public string OAuthAuthorizeUrl { get; set; }

        public string OAuthTokenUrl { get; set; }
    }
}