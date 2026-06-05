using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Data.Repositories
{
    public interface IPendingChargeRepository
    {
        PendingChargeDto GetById(int pendingChargeId);

        IList<PendingChargeDto> GetPendingForPatientCase(int patientId, int patientCaseId);

        IList<PendingChargeDto> GetByCareItemId(int careItemId);

        int Insert(PendingChargeDto charge);

        void Update(PendingChargeDto charge);

        void MarkApproved(int pendingChargeId, int userId);

        void MarkRejected(int pendingChargeId, int userId, string reason);

        void MarkPosted(int pendingChargeId, int postingId);
    }
}