using System;
using System.Collections.Generic;
using System.Linq;
using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;

namespace DBC.CareCommerce.Data.InMemory
{
    public class InMemoryPendingChargeRepository : IPendingChargeRepository
    {
        private readonly List<PendingChargeDto> _items;
        private int _nextId;

        public InMemoryPendingChargeRepository()
        {
            _items = new List<PendingChargeDto>();
            _nextId = 1;
        }

        public PendingChargeDto GetById(int pendingChargeId)
        {
            return _items.FirstOrDefault(x => x.PendingChargeId == pendingChargeId);
        }

        public IList<PendingChargeDto> GetPendingForPatientCase(int patientId, int patientCaseId)
        {
            return _items
                .Where(x =>
                    x.Active &&
                    x.PatientId == patientId &&
                    x.PatientCaseId == patientCaseId &&
                    x.Status == PendingChargeStatus.Pending)
                .ToList();
        }

        public IList<PendingChargeDto> GetByCareItemId(int careItemId)
        {
            return _items
                .Where(x =>
                    x.Active &&
                    x.CareItemId.HasValue &&
                    x.CareItemId.Value == careItemId)
                .ToList();
        }

        public int Insert(PendingChargeDto charge)
        {
            if (charge == null)
            {
                throw new ArgumentNullException("charge");
            }

            if (charge.PendingChargeId.HasValue && GetById(charge.PendingChargeId.Value) != null)
            {
                throw new InvalidOperationException("Pending charge already exists.");
            }

            if (!charge.PendingChargeId.HasValue || charge.PendingChargeId.Value <= 0)
            {
                charge.PendingChargeId = _nextId;
                _nextId += 1;
            }
            else if (charge.PendingChargeId.Value >= _nextId)
            {
                _nextId = charge.PendingChargeId.Value + 1;
            }

            if (charge.PendingChargeGuid == Guid.Empty)
            {
                charge.PendingChargeGuid = Guid.NewGuid();
            }

            if (charge.CreatedDateTime == DateTime.MinValue)
            {
                charge.CreatedDateTime = DateTime.UtcNow;
            }

            _items.Add(charge);

            return charge.PendingChargeId.Value;
        }

        public void Update(PendingChargeDto charge)
        {
            if (charge == null)
            {
                throw new ArgumentNullException("charge");
            }

            if (!charge.PendingChargeId.HasValue)
            {
                throw new InvalidOperationException("PendingChargeId is required for update.");
            }

            var existing = GetById(charge.PendingChargeId.Value);

            if (existing == null)
            {
                throw new InvalidOperationException("Pending charge was not found.");
            }

            var index = _items.IndexOf(existing);
            charge.UpdatedDateTime = DateTime.UtcNow;
            _items[index] = charge;
        }

        public void MarkApproved(int pendingChargeId, int userId)
        {
            var charge = GetRequired(pendingChargeId);

            charge.Status = PendingChargeStatus.Approved;
            charge.ApprovedByUserId = userId;
            charge.ApprovedDateTime = DateTime.UtcNow;
            charge.UpdatedDateTime = DateTime.UtcNow;
        }

        public void MarkRejected(int pendingChargeId, int userId, string reason)
        {
            var charge = GetRequired(pendingChargeId);

            charge.Status = PendingChargeStatus.Rejected;
            charge.RejectedByUserId = userId;
            charge.RejectedDateTime = DateTime.UtcNow;
            charge.RejectionReason = reason;
            charge.UpdatedDateTime = DateTime.UtcNow;
        }

        public void MarkPosted(int pendingChargeId, int postingId)
        {
            var charge = GetRequired(pendingChargeId);

            charge.Status = PendingChargeStatus.Posted;
            charge.PostingId = postingId;
            charge.PostedDateTime = DateTime.UtcNow;
            charge.UpdatedDateTime = DateTime.UtcNow;
        }

        public IList<PendingChargeDto> GetAll()
        {
            return _items.ToList();
        }

        private PendingChargeDto GetRequired(int pendingChargeId)
        {
            var charge = GetById(pendingChargeId);

            if (charge == null)
            {
                throw new InvalidOperationException("Pending charge was not found.");
            }

            return charge;
        }
    }
}