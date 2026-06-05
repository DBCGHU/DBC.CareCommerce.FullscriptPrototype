using System;
using System.Collections.Generic;
using System.Linq;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;

namespace DBC.CareCommerce.Data.InMemory
{
    public class InMemoryFullscriptPatientMapRepository : IFullscriptPatientMapRepository
    {
        private readonly List<FullscriptPatientMapDto> _items;
        private int _nextId;

        public InMemoryFullscriptPatientMapRepository()
        {
            _items = new List<FullscriptPatientMapDto>();
            _nextId = 1;
        }

        public FullscriptPatientMapDto GetById(int fullscriptPatientMapId)
        {
            return _items.FirstOrDefault(x => x.FullscriptPatientMapId == fullscriptPatientMapId);
        }

        public FullscriptPatientMapDto GetByPatientId(int patientId, string environment, string clinicId)
        {
            return _items.FirstOrDefault(x =>
                x.Active &&
                x.PatientId == patientId &&
                string.Equals(x.Environment, environment, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ClinicId ?? string.Empty, clinicId ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        public FullscriptPatientMapDto GetByFullscriptPatientId(string fullscriptPatientId, string environment, string clinicId)
        {
            if (string.IsNullOrWhiteSpace(fullscriptPatientId))
            {
                return null;
            }

            return _items.FirstOrDefault(x =>
                x.Active &&
                string.Equals(x.FullscriptPatientId, fullscriptPatientId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Environment, environment, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ClinicId ?? string.Empty, clinicId ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        public FullscriptPatientMapDto GetByMetadataId(string metadataId, string environment, string clinicId)
        {
            if (string.IsNullOrWhiteSpace(metadataId))
            {
                return null;
            }

            return _items.FirstOrDefault(x =>
                x.Active &&
                string.Equals(x.FullscriptMetadataId, metadataId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Environment, environment, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ClinicId ?? string.Empty, clinicId ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        public IList<FullscriptPatientMapDto> GetAllForPatient(int patientId)
        {
            return _items
                .Where(x => x.PatientId == patientId)
                .ToList();
        }

        public int Insert(FullscriptPatientMapDto map)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            if (map.PatientId <= 0)
            {
                throw new InvalidOperationException("PatientId is required.");
            }

            if (string.IsNullOrWhiteSpace(map.FullscriptPatientId))
            {
                throw new InvalidOperationException("FullscriptPatientId is required.");
            }

            if (string.IsNullOrWhiteSpace(map.Environment))
            {
                throw new InvalidOperationException("Environment is required.");
            }

            if (map.FullscriptPatientMapId.HasValue &&
                GetById(map.FullscriptPatientMapId.Value) != null)
            {
                throw new InvalidOperationException("Fullscript patient map already exists.");
            }

            var existing = GetByPatientId(map.PatientId, map.Environment, map.ClinicId);

            if (existing != null)
            {
                throw new InvalidOperationException("An active Fullscript patient map already exists for this patient/environment/clinic.");
            }

            if (!map.FullscriptPatientMapId.HasValue || map.FullscriptPatientMapId.Value <= 0)
            {
                map.FullscriptPatientMapId = _nextId;
                _nextId += 1;
            }
            else if (map.FullscriptPatientMapId.Value >= _nextId)
            {
                _nextId = map.FullscriptPatientMapId.Value + 1;
            }

            if (map.FullscriptPatientMapGuid == Guid.Empty)
            {
                map.FullscriptPatientMapGuid = Guid.NewGuid();
            }

            if (map.CreatedDateTime == DateTime.MinValue)
            {
                map.CreatedDateTime = DateTime.UtcNow;
            }

            _items.Add(map);

            return map.FullscriptPatientMapId.Value;
        }

        public void Update(FullscriptPatientMapDto map)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            if (!map.FullscriptPatientMapId.HasValue)
            {
                throw new InvalidOperationException("FullscriptPatientMapId is required for update.");
            }

            var existing = GetById(map.FullscriptPatientMapId.Value);

            if (existing == null)
            {
                throw new InvalidOperationException("Fullscript patient map was not found.");
            }

            var index = _items.IndexOf(existing);
            map.UpdatedDateTime = DateTime.UtcNow;
            _items[index] = map;
        }

        public void MarkInactive(int fullscriptPatientMapId)
        {
            var existing = GetById(fullscriptPatientMapId);

            if (existing == null)
            {
                throw new InvalidOperationException("Fullscript patient map was not found.");
            }

            existing.Active = false;
            existing.UpdatedDateTime = DateTime.UtcNow;
        }

        public IList<FullscriptPatientMapDto> GetAll()
        {
            return _items.ToList();
        }
    }
}
