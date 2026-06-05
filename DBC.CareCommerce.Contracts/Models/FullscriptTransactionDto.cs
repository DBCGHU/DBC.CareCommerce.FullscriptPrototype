using System;

namespace DBC.CareCommerce.Contracts.Models
{
    public class FullscriptTransactionDto
    {
        public FullscriptTransactionDto()
        {
            FullscriptTransactionGuid = Guid.NewGuid();
            Status = "Pending";
            Active = true;
            CreatedDateTime = DateTime.UtcNow;
        }

        public int? FullscriptTransactionId { get; set; }
        public Guid FullscriptTransactionGuid { get; set; }

        public int? CareItemId { get; set; }
        public int? CatalogItemId { get; set; }

        public int PatientId { get; set; }
        public int? PatientCaseId { get; set; }
        public int? ProviderId { get; set; }

        public string FullscriptPatientId { get; set; }
        public string FullscriptPractitionerId { get; set; }
        public string FullscriptProductId { get; set; }
        public string FullscriptVariantId { get; set; }

        public string FullscriptTreatmentPlanId { get; set; }
        public string FullscriptOrderId { get; set; }
        public string FullscriptOrderNumber { get; set; }

        public string TreatmentPlanState { get; set; }
        public string OrderStatus { get; set; }

        public string InvitationUrl { get; set; }

        public DateTime? CompletedAt { get; set; }
        public decimal? ItemTotal { get; set; }
        public decimal? MsrpTotal { get; set; }
        public decimal? PaymentTotal { get; set; }

        public DateTime? LastSyncedDateTime { get; set; }

        public string Status { get; set; }
        public string ErrorMessage { get; set; }

        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }

        public bool Active { get; set; }

        public bool HasTreatmentPlan()
        {
            return !string.IsNullOrWhiteSpace(FullscriptTreatmentPlanId);
        }

        public bool HasOrder()
        {
            return !string.IsNullOrWhiteSpace(FullscriptOrderId);
        }
    }
}