using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DBC.Integrations.Fullscript.Configuration;
using DBC.Integrations.Fullscript.Models;

namespace DBC.Integrations.Fullscript.OAuth
{
    public class FullscriptTokenClient
    {
        private readonly HttpClient _httpClient;

        public FullscriptTokenClient()
            : this(new HttpClient())
        {
        }

        public FullscriptTokenClient(HttpClient httpClient)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            _httpClient = httpClient;
        }

        public async Task<FullscriptTokenResponse> ExchangeAuthorizationCodeAsync(
            FullscriptConfiguration configuration,
            string authorizationCode)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            configuration.ValidateForTokenExchange();

            if (string.IsNullOrWhiteSpace(authorizationCode))
            {
                throw new ArgumentException("Authorization code is required.", "authorizationCode");
            }

            var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", authorizationCode },
                { "client_id", configuration.ClientId },
                { "client_secret", configuration.ClientSecret },
                { "redirect_uri", configuration.RedirectUri }
            };

            using (var content = new FormUrlEncodedContent(values))
            {
                using (var response = await _httpClient.PostAsync(configuration.TokenUrl, content).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript token exchange failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return FullscriptTokenResponseParser.Parse(responseText);
                }
            }
        }

        public async Task<FullscriptTokenResponse> RefreshAccessTokenAsync(FullscriptConfiguration configuration, string refreshToken)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            configuration.ValidateForTokenExchange();

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Refresh token is required.", "refreshToken");
            }

            var values = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
                { "client_id", configuration.ClientId },
                { "client_secret", configuration.ClientSecret }
            };

            using (var content = new FormUrlEncodedContent(values))
            {
                using (var response = await _httpClient.PostAsync(configuration.TokenUrl, content).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript token refresh failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return FullscriptTokenResponseParser.Parse(responseText);
                }
            }
        }
    }
}
