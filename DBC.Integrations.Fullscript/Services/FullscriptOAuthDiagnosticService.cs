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
        private readonly IFullscriptOAuthTokenProvider _tokenProvider;

        public FullscriptOAuthDiagnosticService(
            HttpClient httpClient,
            IOptions<FullscriptApiSettings> settings,
            IFullscriptOAuthTokenProvider tokenProvider)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (tokenProvider == null)
            {
                throw new ArgumentNullException("tokenProvider");
            }

            _httpClient = httpClient;
            _settings = settings.Value ?? new FullscriptApiSettings();
            _tokenProvider = tokenProvider;
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
            FullscriptOAuthTokenResult tokenResult = ExchangeCodeForTokenResult(code);

            if (!tokenResult.Success)
            {
                return new
                {
                    success = false,
                    statusCode = tokenResult.StatusCode,
                    errorMessage = tokenResult.ErrorMessage
                };
            }

            return BuildSafeTokenDiagnostic(tokenResult);
        }

        public FullscriptOAuthTokenResult ExchangeCodeForTokenResult(string code)
        {
            string validationError = ValidateTokenExchangeConfiguration(code);

            if (!string.IsNullOrWhiteSpace(validationError))
            {
                return new FullscriptOAuthTokenResult
                {
                    Success = false,
                    ErrorMessage = validationError,
                    ReceivedAtUtc = DateTime.UtcNow
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
                            return new FullscriptOAuthTokenResult
                            {
                                Success = false,
                                StatusCode = (int)response.StatusCode,
                                ErrorMessage = "Fullscript OAuth token exchange failed.",
                                ReceivedAtUtc = DateTime.UtcNow
                            };
                        }

                        FullscriptOAuthTokenResult tokenResult = ParseTokenResult(responseBody);

                        if (tokenResult.Success)
                        {
                            _tokenProvider.StoreToken(tokenResult);
                        }

                        return tokenResult;
                    }
                }
            }
            catch (Exception ex)
            {
                return new FullscriptOAuthTokenResult
                {
                    Success = false,
                    ErrorMessage = "Fullscript OAuth token exchange failed: " + ex.Message,
                    ReceivedAtUtc = DateTime.UtcNow
                };
            }
        }

        public object GetCurrentTokenDiagnostic()
        {
            FullscriptOAuthTokenResult tokenResult = _tokenProvider.GetCurrentToken();

            if (tokenResult == null || !tokenResult.HasAccessToken())
            {
                return new
                {
                    success = true,
                    tokenStored = false,
                    message = "No Fullscript OAuth token is stored in memory for this service process."
                };
            }

            return new
            {
                success = true,
                tokenStored = true,
                tokenType = tokenResult.TokenType,
                expiresIn = tokenResult.ExpiresIn,
                scope = tokenResult.Scope,
                accessTokenPreview = PreviewSecret(tokenResult.AccessToken),
                refreshTokenPresent = tokenResult.HasRefreshToken(),
                receivedAtUtc = tokenResult.ReceivedAtUtc,
                message = "A Fullscript OAuth token is stored in memory for this service process. Full tokens were not returned by this diagnostic endpoint."
            };
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

        private static FullscriptOAuthTokenResult ParseTokenResult(string responseBody)
        {
            using (JsonDocument document = JsonDocument.Parse(responseBody))
            {
                JsonElement root = document.RootElement;
                JsonElement tokenContainer = FindTokenContainer(root);

                FullscriptOAuthTokenResult result = new FullscriptOAuthTokenResult
                {
                    Success = true,
                    AccessToken = FirstNonEmpty(
                        GetStringProperty(tokenContainer, "access_token"),
                        GetStringProperty(tokenContainer, "accessToken")),
                    RefreshToken = FirstNonEmpty(
                        GetStringProperty(tokenContainer, "refresh_token"),
                        GetStringProperty(tokenContainer, "refreshToken")),
                    TokenType = FirstNonEmpty(
                        GetStringProperty(tokenContainer, "token_type"),
                        GetStringProperty(tokenContainer, "tokenType")),
                    ExpiresIn = FirstNonNull(
                        GetIntProperty(tokenContainer, "expires_in"),
                        GetIntProperty(tokenContainer, "expiresIn")),
                    Scope = GetStringProperty(tokenContainer, "scope"),
                    CreatedAt = GetLongProperty(tokenContainer, "created_at"),
                    ResourceOwner = GetStringProperty(tokenContainer, "resource_owner"),
                    ReceivedAtUtc = DateTime.UtcNow
                };

                if (!result.HasAccessToken())
                {
                    result.Success = false;
                    result.ErrorMessage = "Fullscript OAuth token exchange response did not contain an access token.";
                }

                return result;
            }
        }

        private static object BuildSafeTokenDiagnostic(FullscriptOAuthTokenResult tokenResult)
        {
            return new
            {
                success = tokenResult.Success,
                tokenStored = true,
                tokenType = tokenResult.TokenType,
                expiresIn = tokenResult.ExpiresIn,
                scope = tokenResult.Scope,
                accessTokenPreview = PreviewSecret(tokenResult.AccessToken),
                accessTokenPresent = tokenResult.HasAccessToken(),
                refreshTokenPresent = tokenResult.HasRefreshToken(),
                createdAt = tokenResult.CreatedAt,
                resourceOwnerPresent = !string.IsNullOrWhiteSpace(tokenResult.ResourceOwner),
                receivedAtUtc = tokenResult.ReceivedAtUtc,
                message = "OAuth token exchange completed and token was stored in memory. Full tokens were not returned by this diagnostic endpoint."
            };
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

        private static long? GetLongProperty(JsonElement root, string propertyName)
        {
            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty(propertyName, out JsonElement property) &&
                property.TryGetInt64(out long value))
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