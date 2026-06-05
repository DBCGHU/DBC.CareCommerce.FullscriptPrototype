using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Mapping
{
    public class CatalogItemMapper
    {
        public CatalogItemDto FromProduct(
            int productId,
            int? feeId,
            string productName,
            decimal? priceSell,
            decimal? onHand)
        {
            var item = new CatalogItemDto
            {
                CatalogItemType = "InventoryProduct",
                DisplayName = productName,
                ProductId = productId,
                FeeId = feeId,
                DefaultChargeAmount = priceSell,
                InventoryEnabled = true,
                TrackQuantity = true,
                DefaultFulfillmentSource = FulfillmentSource.LocalInventory,
                DefaultBillingAction = BillingAction.CreatePendingCharge,
                DefaultInventoryAction = InventoryAction.DecrementOnPost,
                CatalogStatus = onHand.HasValue && onHand.Value <= 0m ? "OutOfStock" : "Active"
            };

            return item;
        }

        public CatalogItemDto FromSupplementList(
            int supplementId,
            string entry,
            string category)
        {
            var item = new CatalogItemDto
            {
                CatalogItemType = "Supplement",
                ClinicalCategory = category,
                DisplayName = entry,
                SupplementId = supplementId,
                InventoryEnabled = false,
                TrackQuantity = false,
                DefaultFulfillmentSource = FulfillmentSource.DocumentationOnly,
                DefaultBillingAction = BillingAction.NoBilling,
                DefaultInventoryAction = InventoryAction.None,
                RequiresDosage = true
            };

            return item;
        }

        public CatalogItemDto FromFullscriptVariant(
            string productId,
            string variantId,
            string productName,
            string brandName,
            string sku,
            decimal? msrp)
        {
            var item = new CatalogItemDto
            {
                CatalogItemType = "Supplement",
                DisplayName = productName,
                BrandName = brandName,
                FullscriptEnabled = true,
                FullscriptProductId = productId,
                FullscriptVariantId = variantId,
                FullscriptSku = sku,
                FullscriptMsrp = msrp,
                DefaultFulfillmentSource = FulfillmentSource.Fullscript,
                DefaultBillingAction = BillingAction.ExternalPayment,
                DefaultInventoryAction = InventoryAction.None,
                RequiresDosage = true
            };

            return item;
        }
    }
}