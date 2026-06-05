using System;
using System.Collections.Generic;
using System.Linq;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;

namespace DBC.CareCommerce.Data.InMemory
{
    public class InMemoryCareItemRepository : ICareItemRepository
    {
        private readonly List<CareItemDto> _items;
        private int _nextId;

        public InMemoryCareItemRepository()
        {
            _items = new List<CareItemDto>();
            _nextId = 1;
        }

        public CareItemDto GetById(int careItemId)
        {
            return _items.FirstOrDefault(x => x.CareItemId == careItemId);
        }

        public IList<CareItemDto> GetByPatientCase(int patientId, int patientCaseId)
        {
            return _items
                .Where(x =>
                    x.Active &&
                    x.PatientId == patientId &&
                    x.PatientCaseId.HasValue &&
                    x.PatientCaseId.Value == patientCaseId)
                .ToList();
        }

        public IList<CareItemDto> GetByVisit(int visitId)
        {
            return _items
                .Where(x =>
                    x.Active &&
                    x.VisitId.HasValue &&
                    x.VisitId.Value == visitId)
                .ToList();
        }

        public int Insert(CareItemDto item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.CareItemId.HasValue && GetById(item.CareItemId.Value) != null)
            {
                throw new InvalidOperationException("Care item already exists.");
            }

            if (!item.CareItemId.HasValue || item.CareItemId.Value <= 0)
            {
                item.CareItemId = _nextId;
                _nextId += 1;
            }
            else if (item.CareItemId.Value >= _nextId)
            {
                _nextId = item.CareItemId.Value + 1;
            }

            if (item.CareItemGuid == Guid.Empty)
            {
                item.CareItemGuid = Guid.NewGuid();
            }

            if (item.CreatedDateTime == DateTime.MinValue)
            {
                item.CreatedDateTime = DateTime.UtcNow;
            }

            _items.Add(item);

            return item.CareItemId.Value;
        }

        public void Update(CareItemDto item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (!item.CareItemId.HasValue)
            {
                throw new InvalidOperationException("CareItemId is required for update.");
            }

            var existing = GetById(item.CareItemId.Value);

            if (existing == null)
            {
                throw new InvalidOperationException("Care item was not found.");
            }

            var index = _items.IndexOf(existing);
            item.UpdatedDateTime = DateTime.UtcNow;
            _items[index] = item;
        }

        public IList<CareItemDto> GetAll()
        {
            return _items.ToList();
        }
    }
}
