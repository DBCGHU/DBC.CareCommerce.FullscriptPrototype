using System;
using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Mapping;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Services;

namespace DBC.CareCommerce.Contracts.Scenarios
{
    public class SampleWorkflowScenarios
    {
        private readonly CatalogItemMapper _catalogMapper;
        private readonly CareCommerceWorkflowService _workflowService;

        public SampleWorkflowScenarios()
        {
            _catalogMapper = new CatalogItemMapper();
            _workflowService = new CareCommerceWorkflowService();
        }

        public void RunAll()
        {
            RunLocalInventorySupplementScenario();
            RunFullscriptSupplementScenario();
            RunDocumentationOnlyScenario();
            RunInvalidFullscriptScenario();
        }

        public WorkflowDecisionDto RunLocalInventorySupplementScenario()
        {
            var catalogItem = _catalogMapper.FromProduct(
                44,
                1205,
                "Vitamin D3 5000 IU",
                24.99m,
                12m);

            catalogItem.CatalogItemId = 1;

            var availability = new InventoryAvailabilityDto
            {
                CatalogItemId = 1,
                ProductId = 44,
                LocationId = 1,
                OnHand = 12m,
                Reserved = 2m,
                ReorderLevel = 5m
            };

            availability.StockStatus = availability.GetCalculatedStockStatus();

            var careItem = new CareItemDto
            {
                PatientId = 5001,
                PatientCaseId = 9001,
                VisitId = 30022,
                ProviderId = 17,
                CatalogItemId = catalogItem.CatalogItemId,
                SourceSystem = "EHR",
                SourceEntityType = "Supplement",
                SourceEntityId = 81233,
                CareItemType = "Supplement",
                FulfillmentSource = FulfillmentSource.LocalInventory,
                BillingIntent = BillingAction.CreatePendingCharge,
                InventoryIntent = InventoryAction.DecrementOnPost,
                ProductId = catalogItem.ProductId,
                FeeId = catalogItem.FeeId,
                QuantityRecommended = 1m,
                QuantityDispensed = 1m,
                DosageAmount = "1",
                DosageFrequency = "once per day",
                DosageDuration = "30 days",
                DosageFormat = "capsule"
            };

            var decision = _workflowService.EvaluateCareItem(careItem, catalogItem, availability);

            if (!decision.IsSuccessful)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, decision.Errors));
            }

            if (decision.PendingCharge == null)
            {
                throw new InvalidOperationException("Local inventory scenario should create a pending charge.");
            }

            if (decision.FullscriptTransaction != null)
            {
                throw new InvalidOperationException("Local inventory scenario should not create a Fullscript transaction.");
            }

            return decision;
        }

        public WorkflowDecisionDto RunFullscriptSupplementScenario()
        {
            var catalogItem = _catalogMapper.FromFullscriptVariant(
                "fs_prod_123",
                "fs_var_456",
                "Vitamin D3 5000 IU",
                "Example Brand",
                "D3-5000",
                22.5m);

            catalogItem.CatalogItemId = 2;

            var careItem = new CareItemDto
            {
                PatientId = 5001,
                PatientCaseId = 9001,
                VisitId = 30022,
                ProviderId = 17,
                CatalogItemId = catalogItem.CatalogItemId,
                SourceSystem = "EHR",
                SourceEntityType = "Supplement",
                SourceEntityId = 81234,
                CareItemType = "Supplement",
                FulfillmentSource = FulfillmentSource.Fullscript,
                BillingIntent = BillingAction.ExternalPayment,
                InventoryIntent = InventoryAction.None,
                FullscriptVariantId = catalogItem.FullscriptVariantId,
                QuantityRecommended = 1m,
                QuantityDispensed = 0m,
                DosageAmount = "1",
                DosageFrequency = "once per day",
                DosageDuration = "30 days",
                DosageFormat = "capsule"
            };

            var decision = _workflowService.EvaluateCareItem(careItem, catalogItem);

            if (!decision.IsSuccessful)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, decision.Errors));
            }

            if (decision.PendingCharge != null)
            {
                throw new InvalidOperationException("External payment Fullscript scenario should not create a pending charge.");
            }

            if (decision.FullscriptTransaction == null)
            {
                throw new InvalidOperationException("Fullscript scenario should create a Fullscript transaction decision.");
            }

            if (decision.ShouldAffectLocalInventory)
            {
                throw new InvalidOperationException("Fullscript scenario should not affect local inventory.");
            }

            return decision;
        }

        public WorkflowDecisionDto RunDocumentationOnlyScenario()
        {
            var catalogItem = _catalogMapper.FromSupplementList(
                18,
                "Magnesium Glycinate",
                "Nutrition");

            catalogItem.CatalogItemId = 3;

            var careItem = new CareItemDto
            {
                PatientId = 5001,
                PatientCaseId = 9001,
                VisitId = 30022,
                ProviderId = 17,
                CatalogItemId = catalogItem.CatalogItemId,
                SourceSystem = "EHR",
                SourceEntityType = "Supplement",
                SourceEntityId = 81235,
                CareItemType = "Supplement",
                FulfillmentSource = FulfillmentSource.DocumentationOnly,
                BillingIntent = BillingAction.NoBilling,
                InventoryIntent = InventoryAction.None,
                QuantityRecommended = 1m,
                DosageAmount = "1",
                DosageFrequency = "every night",
                DosageDuration = "ongoing",
                DosageFormat = "capsule"
            };

            var decision = _workflowService.EvaluateCareItem(careItem, catalogItem);

            if (!decision.IsSuccessful)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, decision.Errors));
            }

            if (decision.PendingCharge != null)
            {
                throw new InvalidOperationException("Documentation-only scenario should not create a pending charge.");
            }

            if (decision.FullscriptTransaction != null)
            {
                throw new InvalidOperationException("Documentation-only scenario should not create a Fullscript transaction.");
            }

            return decision;
        }

        public WorkflowDecisionDto RunInvalidFullscriptScenario()
        {
            var catalogItem = _catalogMapper.FromFullscriptVariant(
                "fs_prod_123",
                "",
                "Vitamin D3 5000 IU",
                "Example Brand",
                "D3-5000",
                22.5m);

            catalogItem.CatalogItemId = 4;

            var careItem = new CareItemDto
            {
                PatientId = 5001,
                PatientCaseId = 9001,
                CareItemType = "Supplement",
                FulfillmentSource = FulfillmentSource.Fullscript,
                BillingIntent = BillingAction.ExternalPayment,
                InventoryIntent = InventoryAction.None
            };

            var decision = _workflowService.EvaluateCareItem(careItem, catalogItem);

            if (decision.IsSuccessful)
            {
                throw new InvalidOperationException("Invalid Fullscript scenario should have failed validation.");
            }

            return decision;
        }
    }
}