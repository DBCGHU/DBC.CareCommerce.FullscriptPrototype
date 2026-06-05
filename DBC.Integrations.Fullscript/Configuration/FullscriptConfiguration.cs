using System;

namespace DBC.Integrations.Fullscript.Configuration
{
    public class FullscriptConfiguration
    {
        public FullscriptConfiguration()
        {
            Environment = FullscriptEnvironment.UsSandbox;
            RedirectUri = "http://localhost:5000/fullscript/oauth/callback";
        }

        public FullscriptEnvironment Environment { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }

        public string Scope { get; set; }

        public string AuthorizeUrl
        {
            get { return GetAuthorizeUrl(Environment); }
        }

        public string TokenUrl
        {
            get { return GetTokenUrl(Environment); }
        }

        public string ApiBaseUrl
        {
            get { return GetApiBaseUrl(Environment); }
        }

        public void ValidateForAuthorizationUrl()
        {
            if (string.IsNullOrWhiteSpace(ClientId))
            {
                throw new InvalidOperationException("Fullscript ClientId is required.");
            }

            if (string.IsNullOrWhiteSpace(RedirectUri))
            {
                throw new InvalidOperationException("Fullscript RedirectUri is required.");
            }
        }

        public void ValidateForTokenExchange()
        {
            ValidateForAuthorizationUrl();

            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                throw new InvalidOperationException("Fullscript ClientSecret is required.");
            }
        }

        private static string GetAuthorizeUrl(FullscriptEnvironment environment)
        {
            switch (environment)
            {
                case FullscriptEnvironment.UsSandbox:
                    return "https://api-us-snd.fullscript.io/api/oauth/authorize";

                case FullscriptEnvironment.UsProduction:
                    return "https://api-us.fullscript.io/api/oauth/authorize";

                case FullscriptEnvironment.CanadaSandbox:
                    return "https://api-ca-snd.fullscript.io/api/oauth/authorize";

                case FullscriptEnvironment.CanadaProduction:
                    return "https://api-ca.fullscript.io/api/oauth/authorize";

                default:
                    return "https://api-us-snd.fullscript.io/api/oauth/authorize";
            }
        }

        private static string GetTokenUrl(FullscriptEnvironment environment)
        {
            switch (environment)
            {
                case FullscriptEnvironment.UsSandbox:
                    return "https://api-us-snd.fullscript.io/api/oauth/token";

                case FullscriptEnvironment.UsProduction:
                    return "https://api-us.fullscript.io/api/oauth/token";

                case FullscriptEnvironment.CanadaSandbox:
                    return "https://api-ca-snd.fullscript.io/api/oauth/token";

                case FullscriptEnvironment.CanadaProduction:
                    return "https://api-ca.fullscript.io/api/oauth/token";

                default:
                    return "https://api-us-snd.fullscript.io/api/oauth/token";
            }
        }

        private static string GetApiBaseUrl(FullscriptEnvironment environment)
        {
            switch (environment)
            {
                case FullscriptEnvironment.UsSandbox:
                    return "https://api-us-snd.fullscript.io";

                case FullscriptEnvironment.UsProduction:
                    return "https://api-us.fullscript.io";

                case FullscriptEnvironment.CanadaSandbox:
                    return "https://api-ca-snd.fullscript.io";

                case FullscriptEnvironment.CanadaProduction:
                    return "https://api-ca.fullscript.io";

                default:
                    return "https://api-us-snd.fullscript.io";
            }
        }
    }
}