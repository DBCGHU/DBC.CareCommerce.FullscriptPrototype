using System;
using System.Collections.Generic;
using System.Text;
using DBC.Integrations.Fullscript.Configuration;

namespace DBC.Integrations.Fullscript.OAuth
{
    public class FullscriptAuthorizeUrlBuilder
    {
        public string BuildAuthorizeUrl(FullscriptConfiguration configuration, string state)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            configuration.ValidateForAuthorizationUrl();

            var parameters = new Dictionary<string, string>
            {
                { "response_type", "code" },
                { "client_id", configuration.ClientId },
                { "redirect_uri", configuration.RedirectUri }
            };

            if (!string.IsNullOrWhiteSpace(configuration.Scope))
            {
                parameters.Add("scope", configuration.Scope);
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                parameters.Add("state", state);
            }

            return configuration.AuthorizeUrl + "?" + BuildQueryString(parameters);
        }

        private static string BuildQueryString(IDictionary<string, string> parameters)
        {
            var builder = new StringBuilder();
            var first = true;

            foreach (var pair in parameters)
            {
                if (!first)
                {
                    builder.Append("&");
                }

                builder.Append(Uri.EscapeDataString(pair.Key));
                builder.Append("=");
                builder.Append(Uri.EscapeDataString(pair.Value));

                first = false;
            }

            return builder.ToString();
        }
    }
}
