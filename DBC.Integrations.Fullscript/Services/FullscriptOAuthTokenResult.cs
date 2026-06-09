using System;

namespace DBC.Integrations.Fullscript.Services
{
    public sealed class FullscriptOAuthTokenResult
    {
        public bool Success { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int? ExpiresIn { get; set; }
        public string Scope { get; set; }
        public long? CreatedAt { get; set; }
        public string ResourceOwner { get; set; }
        public string ErrorMessage { get; set; }
        public int? StatusCode { get; set; }
        public DateTime ReceivedAtUtc { get; set; }

        public bool HasAccessToken()
        {
            return !string.IsNullOrWhiteSpace(AccessToken);
        }

        public bool HasRefreshToken()
        {
            return !string.IsNullOrWhiteSpace(RefreshToken);
        }
    }
}