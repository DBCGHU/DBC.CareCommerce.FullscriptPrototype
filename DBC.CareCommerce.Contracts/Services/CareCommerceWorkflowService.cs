using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Mapping;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Validation;

namespace DBC.CareCommerce.Contracts.Services
{
    public class CareCommerceWorkflowService
    {
        private readonly CareItemValidator _careItemValidator;
        private readonly PendingChargeValidator _pendingChargeValidator;
        private readonly PendingChargeMapper _pendingChargeMapper;

        public CareCommerceWorkflowService()
        {
            _careItemValidator = new CareItemValidator();
            _pendingChargeValidator = new PendingChargeValidator();
            _pendingChargeMapper = new PendingChargeMapper();
        }

        public WorkflowDecisionDto EvaluateCareItem(
            CareItemDto careItem,
            CatalogItemDto catalogItem,
            InventoryAvailabilityDto availability)
        {
            var decision = new WorkflowDecisionDto
            {
                CareItem = careItem,
                CatalogItem = catalogItem,
                InventoryAvailability = availability
            };

            if (careItem == null)
            {
                decision.AddError("Care item is required.");
                return decision;
            }

            if (catalogItem == null)
            {
                decision.AddError("Catalog item is required.");
                return decision;
            }

            ApplyCatalogDefaultsIfNeeded(careItem, catalogItem);

            decision.FulfillmentSource = careItem.FulfillmentSource;
            decision.BillingAction = careItem.BillingIntent;
            decision.InventoryAction = careItem.InventoryIntent;

            var careValidation = _careItemValidator.Validate(careItem);

            foreach (var validationError in careValidation.Errors)
            {
                decision.AddError(validationError);
            }

            foreach (var validationWarning in careValidation.Warnings)
            {
                decision.AddWarning(validationWarning);
            }

            if (!decision.IsSuccessful)
            {
                decision.AddMessage("Care item failed validation.");
                return decision;
            }

            if (careItem.FulfillmentSource == FulfillmentSource.LocalInventory)
            {
                EvaluateLocalInventoryWorkflow(decision, availability);
            }
            else if (careItem.FulfillmentSource == FulfillmentSource.Fullscript)
            {
                EvaluateFullscriptWorkflow(decision);
            }
            else if (careItem.FulfillmentSource == FulfillmentSource.DocumentationOnly)
            {
                EvaluateDocumentationOnlyWorkflow(decision);
            }
            else if (careItem.FulfillmentSource == FulfillmentSource.ExternalNonStock)
            {
                EvaluateExternalNonStockWorkflow(decision);
            }
            else
            {
                EvaluateNoFulfillmentWorkflow(decision);
            }

            if (decision.ShouldCreatePendingCharge)
            {
                CreatePendingChargeDecision(decision);
            }

            return decision;
        }

        public WorkflowDecisionDto EvaluateCareItem(
            CareItemDto careItem,
            CatalogItemDto catalogItem)
        {
            return EvaluateCareItem(careItem, catalogItem, null);
        }

        private void ApplyCatalogDefaultsIfNeeded(CareItemDto careItem, CatalogItemDto catalogItem)
        {
            if (careItem.FulfillmentSource == FulfillmentSource.None)
            {
                careItem.FulfillmentSource = catalogItem.DefaultFulfillmentSource;
            }

            if (careItem.BillingIntent == BillingAction.NoBilling &&
                catalogItem.DefaultBillingAction != BillingAction.NoBilling)
            {
                careItem.BillingIntent = catalogItem.DefaultBillingAction;
            }

            if (careItem.InventoryIntent == InventoryAction.None &&
                catalogItem.DefaultInventoryAction != InventoryAction.None)
            {
                careItem.InventoryIntent = catalogItem.DefaultInventoryAction;
            }

            if (!careItem.ProductId.HasValue && catalogItem.ProductId.HasValue)
            {
                careItem.ProductId = catalogItem.ProductId;
            }

            if (!careItem.FeeId.HasValue && catalogItem.FeeId.HasValue)
            {
                careItem.FeeId = catalogItem.FeeId;
            }

            if (string.IsNullOrWhiteSpace(careItem.FullscriptVariantId) &&
                !string.IsNullOrWhiteSpace(catalogItem.FullscriptVariantId))
            {
                careItem.FullscriptVariantId = catalogItem.FullscriptVariantId;
            }
        }

        private void EvaluateLocalInventoryWorkflow(
            WorkflowDecisionDto decision,
            InventoryAvailabilityDto availability)
        {
            var item = decision.CareItem;

            decision.ShouldAffectLocalInventory =
                item.InventoryIntent == InventoryAction.Reserve ||
                item.InventoryIntent == InventoryAction.DecrementOnPost ||
                item.InventoryIntent == InventoryAction.DecrementImmediately;

            decision.ShouldCreatePendingCharge =
                item.BillingIntent == BillingAction.CreatePendingCharge;

            decision.ShouldCreatePostingImmediately =
                item.BillingIntent == BillingAction.CreatePostingImmediately;

            if (availability != null)
            {
                var requestedQuantity = 1m;

                if (item.QuantityDispensed.HasValue && item.QuantityDispensed.Value > 0m)
                {
                    requestedQuantity = item.QuantityDispensed.Value;
                }
                else if (item.QuantityRecommended.HasValue && item.QuantityRecommended.Value > 0m)
                {
                    requestedQuantity = item.QuantityRecommended.Value;
                }

                if (!availability.IsInStock(requestedQuantity))
                {
                    decision.AddWarning("Local inventory item does not have enough available stock.");
                }

                if (availability.IsLowStock())
                {
                    decision.AddWarning("Local inventory item is at or below reorder level.");
                }
            }

            decision.AddMessage("Local inventory workflow selected.");
        }

        private void EvaluateFullscriptWorkflow(WorkflowDecisionDto decision)
        {
            var item = decision.CareItem;

            decision.ShouldCreateFullscriptTransaction = true;
            decision.ShouldAffectLocalInventory = false;
            decision.ShouldCreatePostingImmediately = false;

            if (item.BillingIntent == BillingAction.CreatePendingCharge)
            {
                decision.ShouldCreatePendingCharge = true;
                decision.AddWarning("Fullscript item is configured to create a pending charge. Confirm accounting workflow before posting.");
            }
            else
            {
                decision.ShouldCreatePendingCharge = false;
            }

            decision.FullscriptTransaction = new FullscriptTransactionDto
            {
                CareItemId = item.CareItemId,
                CatalogItemId = item.CatalogItemId,
                PatientId = item.PatientId,
                PatientCaseId = item.PatientCaseId,
                ProviderId = item.ProviderId,
                FullscriptProductId = decision.CatalogItem.FullscriptProductId,
                FullscriptVariantId = item.FullscriptVariantId,
                Status = "ReadyToSend"
            };

            decision.AddMessage("Fullscript fulfillment workflow selected.");
        }

        private void EvaluateDocumentationOnlyWorkflow(WorkflowDecisionDto decision)
        {
            decision.IsDocumentationOnly = true;
            decision.ShouldCreatePendingCharge = false;
            decision.ShouldCreateFullscriptTransaction = false;
            decision.ShouldAffectLocalInventory = false;
            decision.ShouldCreatePostingImmediately = false;

            decision.AddMessage("Documentation-only workflow selected.");
        }

        private void EvaluateExternalNonStockWorkflow(WorkflowDecisionDto decision)
        {
            decision.ShouldCreatePendingCharge =
                decision.CareItem.BillingIntent == BillingAction.CreatePendingCharge;

            decision.ShouldCreateFullscriptTransaction = false;
            decision.ShouldAffectLocalInventory = false;

            decision.ShouldCreatePostingImmediately =
                decision.CareItem.BillingIntent == BillingAction.CreatePostingImmediately;

            decision.AddMessage("External non-stock workflow selected.");
        }

        private void EvaluateNoFulfillmentWorkflow(WorkflowDecisionDto decision)
        {
            decision.ShouldCreatePendingCharge =
                decision.CareItem.BillingIntent == BillingAction.CreatePendingCharge;

            decision.ShouldCreateFullscriptTransaction = false;
            decision.ShouldAffectLocalInventory = false;

            decision.ShouldCreatePostingImmediately =
                decision.CareItem.BillingIntent == BillingAction.CreatePostingImmediately;

            decision.AddMessage("No fulfillment workflow selected.");
        }

        private void CreatePendingChargeDecision(WorkflowDecisionDto decision)
        {
            var pendingCharge = _pendingChargeMapper.FromCareItem(decision.CareItem, decision.CatalogItem);
            var validation = _pendingChargeValidator.Validate(pendingCharge);

            foreach (var validationError in validation.Errors)
            {
                decision.AddError(validationError);
            }

            foreach (var validationWarning in validation.Warnings)
            {
                decision.AddWarning(validationWarning);
            }

            decision.PendingCharge = pendingCharge;

            if (validation.IsValid)
            {
                decision.AddMessage("Pending charge created by workflow decision.");
            }
            else
            {
                decision.AddMessage("Pending charge was created but failed validation.");
            }
        }
    }
}