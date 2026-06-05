using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Data.Repositories
{
    public interface IFullscriptPatientMapRepository
    {
        FullscriptPatientMapDto GetById(int fullscriptPatientMapId);

        FullscriptPatientMapDto GetByPatientId(int patientId, string environment, string clinicId);

        FullscriptPatientMapDto GetByFullscriptPatientId(string fullscriptPatientId, string environment, string clinicId);

        FullscriptPatientMapDto GetByMetadataId(string metadataId, string environment, string clinicId);

        IList<FullscriptPatientMapDto> GetAllForPatient(int patientId);

        int Insert(FullscriptPatientMapDto map);

        void Update(FullscriptPatientMapDto map);

        void MarkInactive(int fullscriptPatientMapId);
    }
}