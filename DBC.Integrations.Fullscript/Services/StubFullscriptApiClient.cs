using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Services;

namespace DBC.Integrations.Fullscript.Services
{
    public sealed class StubFullscriptApiClient : IFullscriptApiClient
    {
        public FullscriptDispatchResultDto DispatchTreatmentPlan(
            FullscriptTransactionDto transaction)
        {
            if (transaction == null ||
                !transaction.FullscriptTransactionId.HasValue)
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript transaction ID is required before dispatching."
                };
            }

            return new FullscriptDispatchResultDto
            {
                Success = true,
                ExternalReferenceId = "stub-treatment-plan-" + transaction.FullscriptTransactionId.Value,
                ErrorMessage = null
            };
        }

        public FullscriptPatientCreateResultDto CreatePatient(
            FullscriptPatientCreateRequestDto request)
        {
            if (request == null ||
                request.PatientId <= 0)
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Patient ID is required before creating a Fullscript patient."
                };
            }

            return new FullscriptPatientCreateResultDto
            {
                Success = true,
                FullscriptPatientId = "stub-patient-" + request.PatientId,
                ErrorMessage = null
            };
        }
    }
}