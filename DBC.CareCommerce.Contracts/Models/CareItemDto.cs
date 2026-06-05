using System;
using DBC.CareCommerce.Contracts.Enums;

namespace DBC.CareCommerce.Contracts.Models
{
    public class CareItemDto
    {
        public CareItemDto()
        {
            CareItemGuid = Guid.NewGuid();
            SourceSystem = "Unknown";
            CareItemType = "Unknown";
            ClinicalStatus = CareItemStatus.Draft;
            FulfillmentSource = FulfillmentSource.None;
            BillingIntent = BillingAction.NoBilling;
            InventoryIntent = InventoryAction.None;
            RequiresPatientCase = true;
            Active = true;
            CreatedDateTime = DateTime.UtcNow;
        }

        public int? CareItemId { get; set; }
        public Guid CareItemGuid { get; set; }

        public int PatientId { get; set; }
        public int? PatientCaseId { get; set; }
        public int? VisitId { get; set; }
        public int? ProviderId { get; set; }

        public int? CatalogItemId { get; set; }

        public string SourceSystem { get; set; }
        public string SourceEntityType { get; set; }
        public int? SourceEntityId { get; set; }

        public int? TreatmentId { get; set; }
        public int? MedicationId { get; set; }
        public int? SupplementRecordId { get; set; }
        public int? PostingId { get; set; }

        public string CareItemType { get; set; }
        public CareItemStatus ClinicalStatus { get; set; }

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

        public int? ProductId { get; set; }
        public int? FeeId { get; set; }
        public string FullscriptVariantId { get; set; }

        public bool RequiresPatientCase { get; set; }

        public int? CreatedByUserId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }

        public bool Active { get; set; }

        public bool IsFullscriptItem()
        {
            return FulfillmentSource == FulfillmentSource.Fullscript;
        }

        public bool IsLocalInventoryItem()
        {
            return FulfillmentSource == FulfillmentSource.LocalInventory;
        }

        public bool ShouldCreatePendingCharge()
        {
            return BillingIntent == BillingAction.CreatePendingCharge;
        }

        public bool ShouldAffectInventory()
        {
            return InventoryIntent == InventoryAction.Reserve ||
                   InventoryIntent == InventoryAction.DecrementOnPost ||
                   InventoryIntent == InventoryAction.DecrementImmediately;
        }
    }
}