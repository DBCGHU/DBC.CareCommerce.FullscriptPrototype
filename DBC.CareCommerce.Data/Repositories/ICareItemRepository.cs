using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Data.Repositories
{
    public interface ICareItemRepository
    {
        CareItemDto GetById(int careItemId);

        IList<CareItemDto> GetByPatientCase(int patientId, int patientCaseId);

        IList<CareItemDto> GetByVisit(int visitId);

        int Insert(CareItemDto item);

        void Update(CareItemDto item);
    }
}