using DBC.CareCommerce.Contracts.Enums;

namespace DBC.CareCommerce.Contracts.Requests
{
    public class SubmitCareRecommendationRequest
    {
        public SubmitCareRecommendationRequest()
        {
            SourceSystem = "EHR";
            SourceEntityType = "Unknown";
            CareItemType = "Unknown";

            FulfillmentSource = FulfillmentSource.None;
            BillingIntent = BillingAction.NoBilling;
            InventoryIntent = InventoryAction.None;

            RequiresPatientCase = true;
        }

        public int PatientId { get; set; }
        public int? PatientCaseId { get; set; }
        public int? VisitId { get; set; }
        public int? ProviderId { get; set; }

        public int? CatalogItemId { get; set; }
        public int? FeeId { get; set; }
        public int? ProductId { get; set; }
        public int? SupplementId { get; set; }
        public string FullscriptVariantId { get; set; }

        public string SourceSystem { get; set; }
        public string SourceEntityType { get; set; }
        public int? SourceEntityId { get; set; }

        public int? TreatmentId { get; set; }
        public int? MedicationId { get; set; }
        public int? SupplementRecordId { get; set; }
        public int? PostingId { get; set; }

        public string CareItemType { get; set; }

        public FulfillmentSource FulfillmentSource { get; set; }
        public BillingAction BillingIntent { get; set; }
        public InventoryAction InventoryIntent { get; set; }

        public decimal? QuantityRecommended { get; set; }
        public decimal? QuantityDispensed { get; set; }

        public string DosageAmount { get; set; }
        public string DosageFrequency { get; set; }
        public string DosageDuration { get; set; }
        public string DosageFormat { get; set; }
        public string TakeWith { get; set; }
        public string Instructions { get; set; }
        public string NarrativeText { get; set; }

        public bool RequiresPatientCase { get; set; }

        public int? CreatedByUserId { get; set; }
    }
}