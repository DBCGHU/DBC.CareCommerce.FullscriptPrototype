using DBC.CareCommerce.Contracts.Requests;
using DBC.CareCommerce.Contracts.Responses;

namespace DBC.CareCommerce.Contracts.Services
{
    public interface ICareCommerceIntegrationService
    {
        SubmitCareRecommendationResponse SubmitCareRecommendation(
            SubmitCareRecommendationRequest request);
    }
}