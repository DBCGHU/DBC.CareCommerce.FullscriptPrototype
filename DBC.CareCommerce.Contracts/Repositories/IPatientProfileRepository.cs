using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Repositories
{
    public interface IPatientProfileRepository
    {
        FullscriptPatientProfileDto GetByPatientId(int patientId);
    }
}