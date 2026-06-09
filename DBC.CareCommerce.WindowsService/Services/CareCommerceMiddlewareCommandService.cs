using System;
using DBC.CareCommerce.Contracts.Requests;
using DBC.CareCommerce.Contracts.Responses;
using DBC.CareCommerce.Contracts.Services;

namespace DBC.CareCommerce.WindowsService.Services
{
    public sealed class CareCommerceMiddlewareCommandService
    {
        private readonly ICareCommerceIntegrationService _careCommerceIntegrationService;

        public CareCommerceMiddlewareCommandService(
            ICareCommerceIntegrationService careCommerceIntegrationService)
        {
            if (careCommerceIntegrationService == null)
            {
                throw new ArgumentNullException("careCommerceIntegrationService");
            }

            _careCommerceIntegrationService = careCommerceIntegrationService;
        }

        public SubmitCareRecommendationResponse SubmitCareRecommendation(
            SubmitCareRecommendationRequest request)
        {
            return _careCommerceIntegrationService.SubmitCareRecommendation(request);
        }
    }
}
