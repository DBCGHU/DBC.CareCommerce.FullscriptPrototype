using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Services
{
    public interface IFullscriptApiClient
    {
        FullscriptDispatchResultDto DispatchTreatmentPlan(
            FullscriptTransactionDto transaction);

        FullscriptPatientCreateResultDto CreatePatient(
            FullscriptPatientCreateRequestDto request);
    }
}