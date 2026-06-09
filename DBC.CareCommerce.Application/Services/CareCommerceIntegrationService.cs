using DBC.CareCommerce.Contracts.Requests;
using DBC.CareCommerce.Contracts.Responses;
using DBC.CareCommerce.Contracts.Services;
using DBC.CareCommerce.Contracts.Services.Contracts;
using System;

namespace DBC.CareCommerce.Application.Services
{
    public sealed class CareCommerceIntegrationService : ICareCommerceIntegrationService
    {
        private readonly ICareItemApplicationService _careItemApplicationService;

        public CareCommerceIntegrationService(
            ICareItemApplicationService careItemApplicationService)
        {
            if (careItemApplicationService == null)
            {
                throw new ArgumentNullException("careItemApplicationService");
            }

            _careItemApplicationService = careItemApplicationService;
        }

        public SubmitCareRecommendationResponse SubmitCareRecommendation(
            SubmitCareRecommendationRequest request)
        {
            SubmitCareRecommendationResponse response =
                new SubmitCareRecommendationResponse();

            if (request == null)
            {
                response.AddError("SubmitCareRecommendationRequest is required.");
                return response;
            }

            CreateCareItemRequest createCareItemRequest =
                MapToCreateCareItemRequest(request);

            CreateCareItemResponse createCareItemResponse =
                _careItemApplicationService.CreateCareItem(createCareItemRequest);

            response.ApplyCreateCareItemResponse(createCareItemResponse);

            if (response.Success)
            {
                response.AddMessage("Care recommendation submitted through Care Commerce integration service.");
            }

            return response;
        }

        private static CreateCareItemRequest MapToCreateCareItemRequest(
            SubmitCareRecommendationRequest request)
        {
            return new CreateCareItemRequest
            {
                PatientId = request.PatientId,
                PatientCaseId = request.PatientCaseId,
                VisitId = request.VisitId,
                ProviderId = request.ProviderId,

                CatalogItemId = request.CatalogItemId,
                FeeId = request.FeeId,
                ProductId = request.ProductId,
                SupplementId = request.SupplementId,
                FullscriptVariantId = request.FullscriptVariantId,

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

                RequiresPatientCase = request.RequiresPatientCase,

                CreatedByUserId = request.CreatedByUserId
            };
        }
    }
}