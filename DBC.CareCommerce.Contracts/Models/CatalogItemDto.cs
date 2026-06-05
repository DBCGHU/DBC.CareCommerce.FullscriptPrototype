using System;
using DBC.CareCommerce.Contracts.Enums;

namespace DBC.CareCommerce.Contracts.Models
{
    public class CatalogItemDto
    {
        public CatalogItemDto()
        {
            CatalogItemGuid = Guid.NewGuid();
            CatalogItemType = "Unknown";
            DefaultFulfillmentSource = FulfillmentSource.None;
            DefaultBillingAction = BillingAction.NoBilling;
            DefaultInventoryAction = InventoryAction.None;
            CatalogStatus = "Active";
            Active = true;
            RequiresPatient = true;
            RequiresPatientCase = true;
            CreatedDateTime = DateTime.UtcNow;
        }

        public int? CatalogItemId { get; set; }
        public Guid CatalogItemGuid { get; set; }

        public string CatalogItemType { get; set; }
        public string ClinicalCategory { get; set; }
        public string BillingCategory { get; set; }
        public string FulfillmentCategory { get; set; }

        public string DisplayName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string SearchKeywords { get; set; }

        public string BrandName { get; set; }
        public string ManufacturerName { get; set; }
        public string Sku { get; set; }
        public string Upc { get; set; }

        public int? FeeId { get; set; }
        public int? ProductId { get; set; }
        public int? SupplementId { get; set; }

        public decimal? DefaultChargeAmount { get; set; }
        public decimal? DefaultUnits { get; set; }
        public bool? Taxable { get; set; }
        public string RevenueCategory { get; set; }
        public string LedgerCode { get; set; }

        public bool InventoryEnabled { get; set; }
        public bool TrackQuantity { get; set; }
        public int? DefaultInventoryLocationId { get; set; }
        public string UnitOfMeasure { get; set; }
        public string PackageSize { get; set; }
        public decimal? ReorderPoint { get; set; }
        public decimal? ReorderQuantity { get; set; }

        public bool FullscriptEnabled { get; set; }
        public string FullscriptProductId { get; set; }
        public string FullscriptVariantId { get; set; }
        public string FullscriptSku { get; set; }
        public string FullscriptUpc { get; set; }
        public string FullscriptBrandId { get; set; }
        public string FullscriptBrandName { get; set; }
        public string FullscriptProductName { get; set; }
        public string FullscriptVariantStatus { get; set; }
        public string FullscriptAvailability { get; set; }
        public decimal? FullscriptMsrp { get; set; }
        public DateTime? FullscriptLastSyncedDateTime { get; set; }

        public FulfillmentSource DefaultFulfillmentSource { get; set; }
        public BillingAction DefaultBillingAction { get; set; }
        public InventoryAction DefaultInventoryAction { get; set; }

        public bool RequiresPatient { get; set; }
        public bool RequiresPatientCase { get; set; }
        public bool RequiresProvider { get; set; }
        public bool RequiresDosage { get; set; }
        public bool RequiresInstructions { get; set; }

        public string DefaultDosageAmount { get; set; }
        public string DefaultDosageFrequency { get; set; }
        public string DefaultDosageDuration { get; set; }
        public string DefaultDosageFormat { get; set; }
        public string DefaultTakeWith { get; set; }
        public string DefaultInstructions { get; set; }

        public string CatalogStatus { get; set; }
        public string MappingConfidence { get; set; }
        public bool NeedsReview { get; set; }

        public bool Active { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }

        public bool CanUseLocalInventory()
        {
            return InventoryEnabled && ProductId.HasValue;
        }

        public bool CanUseFullscript()
        {
            return FullscriptEnabled && !string.IsNullOrWhiteSpace(FullscriptVariantId);
        }

        public bool IsBillable()
        {
            return DefaultBillingAction != BillingAction.NoBilling &&
                   DefaultBillingAction != BillingAction.ExternalPayment;
        }
    }
}