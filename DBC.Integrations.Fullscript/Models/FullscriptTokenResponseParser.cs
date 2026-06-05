using System;
using System.Text.Json;

namespace DBC.Integrations.Fullscript.Models
{
    public static class FullscriptTokenResponseParser
    {
        public static FullscriptTokenResponse Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Token response JSON is required.", "json");
            }

            using (var document = JsonDocument.Parse(json))
            {
                var root = document.RootElement;

                JsonElement tokenRoot;

                if (root.TryGetProperty("oauth", out tokenRoot))
                {
                    root = tokenRoot;
                }

                var token = new FullscriptTokenResponse
                {
                    AccessToken = GetString(root, "access_token"),
                    RefreshToken = GetString(root, "refresh_token"),
                    TokenType = GetString(root, "token_type"),
                    Scope = GetString(root, "scope"),
                    ExpiresIn = GetNullableInt(root, "expires_in"),
                    ReceivedDateTimeUtc = DateTime.UtcNow
                };

                return token;
            }
        }

        private static string GetString(JsonElement root, string propertyName)
        {
            JsonElement value;

            if (!root.TryGetProperty(propertyName, out value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return value.GetString();
        }

        private static int? GetNullableInt(JsonElement root, string propertyName)
        {
            JsonElement value;

            if (!root.TryGetProperty(propertyName, out value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetInt32();
            }

            int parsed;

            if (int.TryParse(value.GetString(), out parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}