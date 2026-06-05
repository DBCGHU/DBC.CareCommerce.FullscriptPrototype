using System;

namespace DBC.CareCommerce.Contracts.Models
{
    public class FullscriptConnectionDto
    {
        public FullscriptConnectionDto()
        {
            FullscriptConnectionGuid = Guid.NewGuid();
            Environment = "UsSandbox";
            Status = "Active";
            Active = true;
            CreatedDateTime = DateTime.UtcNow;
        }

        public int? FullscriptConnectionId { get; set; }

        public Guid FullscriptConnectionGuid { get; set; }

        public string Environment { get; set; }

        public string ClinicId { get; set; }

        public string ClinicName { get; set; }

        public string PractitionerId { get; set; }

        public string PractitionerType { get; set; }

        public string ClientId { get; set; }

        public string AccessTokenEncrypted { get; set; }

        public string RefreshTokenEncrypted { get; set; }

        public string TokenType { get; set; }

        public string Scope { get; set; }

        public DateTime? TokenReceivedDateTime { get; set; }

        public DateTime? TokenExpiresAtDateTime { get; set; }

        public DateTime? LastRefreshDateTime { get; set; }

        public string DispensaryUrl { get; set; }

        public string IntegrationId { get; set; }

        public DateTime? IntegrationActivatedAt { get; set; }

        public string Status { get; set; }

        public string ErrorMessage { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime? UpdatedDateTime { get; set; }

        public bool HasRefreshToken()
        {
            return !string.IsNullOrWhiteSpace(RefreshTokenEncrypted);
        }

        public bool HasAccessToken()
        {
            return !string.IsNullOrWhiteSpace(AccessTokenEncrypted);
        }

        public bool IsTokenExpiredOrNearExpiry()
        {
            if (!TokenExpiresAtDateTime.HasValue)
            {
                return true;
            }

            return TokenExpiresAtDateTime.Value <= DateTime.UtcNow.AddMinutes(5);
        }
    }
}