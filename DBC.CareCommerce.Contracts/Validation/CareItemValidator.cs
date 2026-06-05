using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Validation
{
    public class CareItemValidator
    {
        public ValidationResult Validate(CareItemDto item)
        {
            var result = new ValidationResult();

            if (item == null)
            {
                result.AddError("Care item is required.");
                return result;
            }

            if (item.PatientId <= 0)
            {
                result.AddError("PatientId is required.");
            }

            if (item.RequiresPatientCase && !item.PatientCaseId.HasValue)
            {
                result.AddError("PatientCaseId is required.");
            }

            if (string.IsNullOrWhiteSpace(item.CareItemType))
            {
                result.AddError("CareItemType is required.");
            }

            if (item.FulfillmentSource == FulfillmentSource.LocalInventory)
            {
                if (!item.ProductId.HasValue)
                {
                    result.AddError("Local inventory fulfillment requires ProductId.");
                }

                if (item.InventoryIntent == InventoryAction.None)
                {
                    result.AddWarning("Local inventory item has no inventory action.");
                }
            }

            if (item.FulfillmentSource == FulfillmentSource.Fullscript)
            {
                if (string.IsNullOrWhiteSpace(item.FullscriptVariantId))
                {
                    result.AddError("Fullscript fulfillment requires FullscriptVariantId.");
                }

                if (item.InventoryIntent != InventoryAction.None &&
                    item.InventoryIntent != InventoryAction.FullscriptOnly)
                {
                    result.AddError("Fullscript fulfillment must not decrement or reserve local inventory.");
                }
            }

            if (item.BillingIntent == BillingAction.CreatePendingCharge ||
                item.BillingIntent == BillingAction.CreatePostingImmediately)
            {
                if (!item.FeeId.HasValue)
                {
                    result.AddWarning("Billable item does not have a FeeId.");
                }
            }

            return result;
        }
    }
}