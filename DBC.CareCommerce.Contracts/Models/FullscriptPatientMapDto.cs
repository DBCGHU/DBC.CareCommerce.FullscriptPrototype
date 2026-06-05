using System;

namespace DBC.CareCommerce.Contracts.Models
{
    public class FullscriptPatientMapDto
    {
        public FullscriptPatientMapDto()
        {
            FullscriptPatientMapGuid = Guid.NewGuid();
            Environment = "UsSandbox";
            Active = true;
            CreatedDateTime = DateTime.UtcNow;
        }

        public int? FullscriptPatientMapId { get; set; }

        public Guid FullscriptPatientMapGuid { get; set; }

        public int PatientId { get; set; }

        public string FullscriptPatientId { get; set; }

        public string FullscriptMetadataId { get; set; }

        public string FullscriptEmail { get; set; }

        public string FullscriptFirstName { get; set; }

        public string FullscriptLastName { get; set; }

        public string Environment { get; set; }

        public string ClinicId { get; set; }

        public DateTime? LastSyncedDateTime { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public DateTime? UpdatedDateTime { get; set; }

        public bool HasFullscriptPatientId()
        {
            return !string.IsNullOrWhiteSpace(FullscriptPatientId);
        }

        public bool MatchesPatient(int patientId)
        {
            return PatientId == patientId;
        }

        public bool MatchesEnvironmentAndClinic(string environment, string clinicId)
        {
            return string.Equals(Environment, environment, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ClinicId ?? string.Empty, clinicId ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }
}