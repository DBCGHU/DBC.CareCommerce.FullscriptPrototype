using System;

namespace DBC.Integrations.Fullscript.Models
{
    public class FullscriptTokenResponse
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string TokenType { get; set; }

        public int? ExpiresIn { get; set; }

        public string Scope { get; set; }

        public DateTime ReceivedDateTimeUtc { get; set; }

        public DateTime? ExpiresAtUtc
        {
            get
            {
                if (!ExpiresIn.HasValue)
                {
                    return null;
                }

                return ReceivedDateTimeUtc.AddSeconds(ExpiresIn.Value);
            }
        }

        public bool HasAccessToken()
        {
            return !string.IsNullOrWhiteSpace(AccessToken);
        }
    }
}
