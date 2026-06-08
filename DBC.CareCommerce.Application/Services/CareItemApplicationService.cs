using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Contracts.Requests;
using DBC.CareCommerce.Contracts.Responses;
using DBC.CareCommerce.Contracts.Services;
using DBC.CareCommerce.Contracts.Services.Contracts;
using DBC.CareCommerce.Data.Repositories;
using System;

namespace DBC.CareCommerce.Application.Services
{
    public class CareItemApplicationService : ICareItemApplicationService
    {
        private readonly ICatalogItemRepository _catalogItemRepository;
        private readonly ICareItemRepository _careItemRepository;
        private readonly IPendingChargeRepository _pendingChargeRepository;
        private readonly CareCommerceWorkflowService _workflowService;
        private readonly IFullscriptTransactionRepository _fullscriptTransactionRepository;

        public CareItemApplicationService(
            ICatalogItemRepository catalogItemRepository,
            ICareItemRepository careItemRepository,
            IPendingChargeRepository pendingChargeRepository,
            IFullscriptTransactionRepository fullscriptTransactionRepository)
        {
            if (catalogItemRepository == null)
            {
                throw new ArgumentNullException("catalogItemRepository");
            }

            if (careItemRepository == null)
            {
                throw new ArgumentNullException("careItemRepository");
            }

            if (pendingChargeRepository == null)
            {
                throw new ArgumentNullException("pendingChargeRepository");
            }

            if (fullscriptTransactionRepository == null)
            {
                throw new ArgumentNullException("fullscriptTransactionRepository");
            }

            _catalogItemRepository = catalogItemRepository;
            _careItemRepository = careItemRepository;
            _pendingChargeRepository = pendingChargeRepository;
            _fullscriptTransactionRepository = fullscriptTransactionRepository;
            _workflowService = new CareCommerceWorkflowService();
        }

        public CreateCareItemResponse CreateCareItem(CreateCareItemRequest request)
        {
            var response = new CreateCareItemResponse();

            if (request == null)
            {
                response.AddError("CreateCareItemRequest is required.");
                return response;
            }

            if (request.PatientId <= 0)
            {
                response.AddError("PatientId is required.");
            }

            if (request.RequiresPatientCase && !request.PatientCaseId.HasValue)
            {
                response.AddError("PatientCaseId is required.");
            }

            if (!request.HasCatalogResolver())
            {
                response.AddError("At least one catalog resolver is required: CatalogItemId, FeeId, ProductId, SupplementId, or FullscriptVariantId.");
            }

            if (response.Errors.Count > 0)
            {
                return response;
            }

            var catalogItem = ResolveCatalogItem(request);

            if (catalogItem == null)
            {
                response.AddError("CatalogItem could not be resolved from the request.");
                return response;
            }

            var careItem = BuildCareItem(request, catalogItem);

            var decision = _workflowService.EvaluateCareItem(careItem, catalogItem);

            if (!decision.IsSuccessful)
            {
                response.ApplyWorkflowDecision(decision);
                return response;
            }

            var careItemId = _careItemRepository.Insert(careItem);
            careItem.CareItemId = careItemId;
            decision.CareItem = careItem;

            if (decision.PendingCharge != null)
            {
                decision.PendingCharge.CareItemId = careItemId;

                var pendingChargeId = _pendingChargeRepository.Insert(decision.PendingCharge);
                decision.PendingCharge.PendingChargeId = pendingChargeId;
            }

            if (decision.FullscriptTransaction != null)
            {
                decision.FullscriptTransaction.CareItemId = careItemId;
                decision.FullscriptTransaction.CatalogItemId = catalogItem.CatalogItemId;
                decision.FullscriptTransaction.PatientId = request.PatientId;
                decision.FullscriptTransaction.PatientCaseId = request.PatientCaseId;
                decision.FullscriptTransaction.ProviderId = request.ProviderId;
                decision.FullscriptTransaction.FullscriptProductId = catalogItem.FullscriptProductId;
                decision.FullscriptTransaction.FullscriptVariantId = catalogItem.FullscriptVariantId;

                var fullscriptTransactionId = _fullscriptTransactionRepository.Insert(decision.FullscriptTransaction);
                decision.FullscriptTransaction.FullscriptTransactionId = fullscriptTransactionId;
            }

            response.ApplyWorkflowDecision(decision);
            response.Success = response.Errors.Count == 0;

            return response;
        }

        private CatalogItemDto ResolveCatalogItem(CreateCareItemRequest request)
        {
            if (request.CatalogItemId.HasValue)
            {
                return _catalogItemRepository.GetById(request.CatalogItemId.Value);
            }

            if (request.FeeId.HasValue)
            {
                return _catalogItemRepository.GetByFeeId(request.FeeId.Value);
            }

            if (request.ProductId.HasValue)
            {
                return _catalogItemRepository.GetByProductId(request.ProductId.Value);
            }

            if (request.SupplementId.HasValue)
            {
                return _catalogItemRepository.GetBySupplementId(request.SupplementId.Value);
            }

            /*
             * FullscriptVariantId lookup is not in ICatalogItemRepository yet.
             * We should add GetByFullscriptVariantId later if needed.
             */

            return null;
        }

        private CareItemDto BuildCareItem(CreateCareItemRequest request, CatalogItemDto catalogItem)
        {
            var careItem = new CareItemDto
            {
                PatientId = request.PatientId,
                PatientCaseId = request.PatientCaseId,
                VisitId = request.VisitId,
                ProviderId = request.ProviderId,

                CatalogItemId = catalogItem.CatalogItemId,

                SourceSystem = request.SourceSystem,
                SourceEntityType = request.SourceEntityType,
                SourceEntityId = request.SourceEntityId,

                TreatmentId = request.TreatmentId,
                MedicationId = request.MedicationId,
                SupplementRecordId = request.SupplementRecordId,
                PostingId = request.PostingId,

                CareItemType = request.CareItemType,

                FulfillmentSource = request.FulfillmentSource,
                BillingIntent = request.BillingIntent,
                InventoryIntent = request.InventoryIntent,

                QuantityRecommended = request.QuantityRecommended,
                QuantityDispensed = request.QuantityDispensed,

                DosageAmount = request.DosageAmount,
                DosageFrequency = request.DosageFrequency,
                DosageDuration = request.DosageDuration,
                DosageFormat = request.DosageFormat,
                TakeWith = request.TakeWith,
                Instructions = request.Instructions,
                NarrativeText = request.NarrativeText,

                ProductId = request.ProductId.HasValue ? request.ProductId : catalogItem.ProductId,
                FeeId = request.FeeId.HasValue ? request.FeeId : catalogItem.FeeId,
                FullscriptVariantId = !string.IsNullOrWhiteSpace(request.FullscriptVariantId)
                    ? request.FullscriptVariantId
                    : catalogItem.FullscriptVariantId,

                RequiresPatientCase = request.RequiresPatientCase,
                CreatedByUserId = request.CreatedByUserId,
                Active = true,
                CreatedDateTime = DateTime.UtcNow
            };

            /*
             * If caller did not explicitly choose workflow settings,
             * let the workflow service apply catalog defaults.
             */
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

            return careItem;
        }
    }
}