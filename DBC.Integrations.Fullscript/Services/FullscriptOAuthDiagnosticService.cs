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
                JsonElement tokenContainer = FindTokenContainer(root);

                string accessToken = GetStringProperty(tokenContainer, "access_token");
                string refreshToken = GetStringProperty(tokenContainer, "refresh_token");

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    accessToken = GetStringProperty(tokenContainer, "accessToken");
                }

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    refreshToken = GetStringProperty(tokenContainer, "refreshToken");
                }

                return new
                {
                    success = true,
                    rootFields = GetPropertyNames(root),
                    tokenContainerFields = GetPropertyNames(tokenContainer),
                    tokenType = FirstNonEmpty(
                        GetStringProperty(tokenContainer, "token_type"),
                        GetStringProperty(tokenContainer, "tokenType")),
                    expiresIn = FirstNonNull(
                        GetIntProperty(tokenContainer, "expires_in"),
                        GetIntProperty(tokenContainer, "expiresIn")),
                    scope = GetStringProperty(tokenContainer, "scope"),
                    accessTokenPreview = PreviewSecret(accessToken),
                    accessTokenPresent = !string.IsNullOrWhiteSpace(accessToken),
                    refreshTokenPresent = !string.IsNullOrWhiteSpace(refreshToken),
                    message = "OAuth token exchange completed. Full tokens were not returned by this diagnostic endpoint."
                };
            }
        }

        private static JsonElement FindTokenContainer(JsonElement root)
        {
            string[] containerNames = new[]
            {
                "data",
                "token",
                "tokens",
                "oauth",
                "result"
            };

            foreach (string containerName in containerNames)
            {
                if (root.TryGetProperty(containerName, out JsonElement container) &&
                    container.ValueKind == JsonValueKind.Object)
                {
                    if (HasTokenLikeProperty(container))
                    {
                        return container;
                    }
                }
            }

            return root;
        }

        private static bool HasTokenLikeProperty(JsonElement element)
        {
            return element.TryGetProperty("access_token", out _) ||
                element.TryGetProperty("accessToken", out _) ||
                element.TryGetProperty("refresh_token", out _) ||
                element.TryGetProperty("refreshToken", out _);
        }

        private static string[] GetPropertyNames(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                return new string[0];
            }

            List<string> propertyNames = new List<string>();

            foreach (JsonProperty property in element.EnumerateObject())
            {
                propertyNames.Add(property.Name);
            }

            return propertyNames.ToArray();
        }

        private static string GetStringProperty(JsonElement root, string propertyName)
        {
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty(propertyName, out JsonElement property))
            {
                return property.ToString();
            }

            return null;
        }

        private static int? GetIntProperty(JsonElement root, string propertyName)
        {
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty(propertyName, out JsonElement property) &&
                property.TryGetInt32(out int value))
            {
                return value;
            }

            return null;
        }

        private static int? FirstNonNull(int? first, int? second)
        {
            return first ?? second;
        }

        private static string FirstNonEmpty(string first, string second)
        {
            if (!string.IsNullOrWhiteSpace(first))
            {
                return first;
            }

            return second;
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