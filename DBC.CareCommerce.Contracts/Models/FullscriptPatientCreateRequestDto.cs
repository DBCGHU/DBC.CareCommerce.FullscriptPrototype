using System;

namespace DBC.CareCommerce.Contracts.Models
{
    public sealed class FullscriptPatientCreateRequestDto
    {
        public int PatientId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string MetadataId { get; set; }
    }
}