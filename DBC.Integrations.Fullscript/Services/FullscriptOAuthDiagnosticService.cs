using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using DBC.Integrations.Fullscript.Configuration;
using Microsoft.Extensions.Options;

namespace DBC.Integrations.Fullscript.Services
{
    public sealed class FullscriptOAuthDiagnosticService
    {
        private readonly HttpClient _httpClient;
        private readonly FullscriptApiSettings _settings;

        public FullscriptOAuthDiagnosticService(
            HttpClient httpClient,
            IOptions<FullscriptApiSettings> settings)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            _httpClient = httpClient;
            _settings = settings.Value ?? new FullscriptApiSettings();
        }

        public string BuildAuthorizeUrl()
        {
            if (string.IsNullOrWhiteSpace(_settings.OAuthAuthorizeUrl))
            {
                return null;
            }

            string authorizeUrl = _settings.OAuthAuthorizeUrl;

            if (authorizeUrl.IndexOf("?", StringComparison.Ordinal) >= 0)
            {
                return authorizeUrl;
            }

            return authorizeUrl.TrimEnd('?') +
                "?response_type=code" +
                "&client_id=" + Uri.EscapeDataString(_settings.OAuthClientId ?? string.Empty) +
                "&redirect_uri=" + Uri.EscapeDataString(_settings.OAuthRedirectUri ?? string.Empty);
        }

        public object ExchangeCodeForToken(string code)
        {
            string validationError = ValidateTokenExchangeConfiguration(code);

            if (!string.IsNullOrWhiteSpace(validationError))
            {
                return new
                {
                    success = false,
                    errorMessage = validationError
                };
            }

            try
            {
                using (FormUrlEncodedContent content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "grant_type", "authorization_code" },
                        { "code", code },
                        { "client_id", _settings.OAuthClientId },
                        { "client_secret", _settings.OAuthClientSecret },
                        { "redirect_uri", _settings.OAuthRedirectUri }
                    }))
                {
                    using (HttpResponseMessage response =
                        _httpClient.PostAsync(_settings.OAuthTokenUrl, content).GetAwaiter().GetResult())
                    {
                        string responseBody =
                            response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        if (!response.IsSuccessStatusCode)
                        {
                            return new
                            {
                                success = false,
                                statusCode = (int)response.StatusCode,
                                errorMessage = "Fullscript OAuth token exchange failed.",
                                responseBody = responseBody
                            };
                        }

                        return BuildSafeTokenDiagnostic(responseBody);
                    }
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    success = false,
                    errorMessage = "Fullscript OAuth token exchange failed: " + ex.Message
                };
            }
        }

        private string ValidateTokenExchangeConfiguration(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return "OAuth authorization code is required.";
            }

            if (string.IsNullOrWhiteSpace(_settings.OAuthClientId))
            {
                return "Fullscript OAuth client ID is not configured.";
            }

            if (string.IsNullOrWhiteSpace(_settings.OAuthClientSecret))
            {
                return "Fullscript OAuth client secret is not configured.";
            }

            if (string.IsNullOrWhiteSpace(_settings.OAuthRedirectUri))
            {
                return "Fullscript OAuth redirect URI is not configured.";
            }

            if (string.IsNullOrWhiteSpace(_settings.OAuthTokenUrl))
            {
                return "Fullscript OAuth token URL is not configured.";
            }

            return null;
        }

        private static object BuildSafeTokenDiagnostic(string responseBody)
        {
            using (JsonDocument document = JsonDocument.Parse(responseBody))
            {
                JsonElement root = document.RootElement;

                string accessToken = GetStringProperty(root, "access_token");
                string refreshToken = GetStringProperty(root, "refresh_token");

                return new
                {
                    success = true,
                    tokenType = GetStringProperty(root, "token_type"),
                    expiresIn = GetIntProperty(root, "expires_in"),
                    scope = GetStringProperty(root, "scope"),
                    accessTokenPreview = PreviewSecret(accessToken),
                    refreshTokenPresent = !string.IsNullOrWhiteSpace(refreshToken),
                    message = "OAuth token exchange completed. Full tokens were not returned by this diagnostic endpoint."
                };
            }
        }

        private static string GetStringProperty(JsonElement root, string propertyName)
        {
            if (root.TryGetProperty(propertyName, out JsonElement property))
            {
                return property.ToString();
            }

            return null;
        }

        private static int? GetIntProperty(JsonElement root, string propertyName)
        {
            if (root.TryGetProperty(propertyName, out JsonElement property) &&
                property.TryGetInt32(out int value))
            {
                return value;
            }

            return null;
        }

        private static string PreviewSecret(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (value.Length <= 8)
            {
                return "****";
            }

            return value.Substring(0, 4) + "..." + value.Substring(value.Length - 4);
        }
    }
}