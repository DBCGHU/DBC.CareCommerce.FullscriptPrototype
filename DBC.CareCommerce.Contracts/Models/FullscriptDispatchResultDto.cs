namespace DBC.CareCommerce.Contracts.Models
{
    public sealed class FullscriptDispatchResultDto
    {
        public bool Success { get; set; }

        public string ExternalReferenceId { get; set; }

        public string ErrorMessage { get; set; }
    }
}