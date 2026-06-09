namespace DBC.CareCommerce.Contracts.Models
{
    public sealed class FullscriptPatientCreateResultDto
    {
        public bool Success { get; set; }
        public string FullscriptPatientId { get; set; }
        public string ErrorMessage { get; set; }
    }
}