using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Validation
{
    public class PendingChargeValidator
    {
        public ValidationResult Validate(PendingChargeDto charge)
        {
            var result = new ValidationResult();

            if (charge == null)
            {
                result.AddError("Pending charge is required.");
                return result;
            }

            if (charge.PatientId <= 0)
            {
                result.AddError("PatientId is required.");
            }

            if (charge.PatientCaseId <= 0)
            {
                result.AddError("PatientCaseId is required.");
            }

            if (string.IsNullOrWhiteSpace(charge.Description))
            {
                result.AddError("Description is required.");
            }

            if (charge.Quantity <= 0m)
            {
                result.AddError("Quantity must be greater than zero.");
            }

            if (charge.BillingAction == BillingAction.CreatePendingCharge &&
                !charge.FeeId.HasValue)
            {
                result.AddWarning("Pending charge does not have a FeeId.");
            }

            if (charge.FulfillmentSource == FulfillmentSource.Fullscript &&
                charge.InventoryAction != InventoryAction.None &&
                charge.InventoryAction != InventoryAction.FullscriptOnly)
            {
                result.AddError("Fullscript charges must not affect local inventory.");
            }

            if (charge.Status == PendingChargeStatus.Posted &&
                !charge.PostingId.HasValue)
            {
                result.AddError("Posted pending charge must have a PostingId.");
            }

            return result;
        }
    }
}