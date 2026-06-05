using DBC.CareCommerce.Contracts.Requests;
using DBC.CareCommerce.Contracts.Responses;

namespace DBC.CareCommerce.Contracts.Services.Contracts
{
    public interface ICareItemApplicationService
    {
        CreateCareItemResponse CreateCareItem(CreateCareItemRequest request);
    }
}