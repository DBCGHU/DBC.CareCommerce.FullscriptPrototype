using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Requests;

namespace DBC.CareCommerce.WindowsService.Services
{
    public sealed class SubmitCareRecommendationRequestValidator
    {
        public List<string> Validate(SubmitCareRecommendationRequest request)
        {
            List<string> errors = new List<string>();

            if (request == null)
            {
                errors.Add("Request body is required.");
                return errors;
            }

            if (request.PatientId <= 0)
            {
                errors.Add("PatientId is required.");
            }

            if (request.RequiresPatientCase && !request.PatientCaseId.HasValue)
            {
                errors.Add("PatientCaseId is required when RequiresPatientCase is true.");
            }

            if (!request.CatalogItemId.HasValue &&
                !request.FeeId.HasValue &&
                !request.ProductId.HasValue &&
                !request.SupplementId.HasValue &&
                string.IsNullOrWhiteSpace(request.FullscriptVariantId))
            {
                errors.Add("At least one item identifier is required: CatalogItemId, FeeId, ProductId, SupplementId, or FullscriptVariantId.");
            }

            if (string.IsNullOrWhiteSpace(request.CareItemType))
            {
                errors.Add("CareItemType is required.");
            }

            if (string.IsNullOrWhiteSpace(request.SourceSystem))
            {
                errors.Add("SourceSystem is required.");
            }

            if (string.IsNullOrWhiteSpace(request.SourceEntityType))
            {
                errors.Add("SourceEntityType is required.");
            }

            return errors;
        }
    }
}