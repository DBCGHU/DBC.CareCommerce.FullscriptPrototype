using System;
using System.Collections.Generic;
using System.Linq;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;

namespace DBC.CareCommerce.Data.InMemory
{
    public class InMemoryFullscriptConnectionRepository : IFullscriptConnectionRepository
    {
        private readonly List<FullscriptConnectionDto> _items;
        private int _nextId;

        public InMemoryFullscriptConnectionRepository()
        {
            _items = new List<FullscriptConnectionDto>();
            _nextId = 1;
        }

        public FullscriptConnectionDto GetById(int fullscriptConnectionId)
        {
            return _items.FirstOrDefault(x => x.FullscriptConnectionId == fullscriptConnectionId);
        }

        public FullscriptConnectionDto GetActiveByEnvironmentAndClinic(string environment, string clinicId)
        {
            return _items.FirstOrDefault(x =>
                x.Active &&
                string.Equals(x.Environment, environment, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ClinicId ?? string.Empty, clinicId ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        public IList<FullscriptConnectionDto> GetActiveByEnvironment(string environment)
        {
            return _items
                .Where(x =>
                    x.Active &&
                    string.Equals(x.Environment, environment, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public int Insert(FullscriptConnectionDto connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (string.IsNullOrWhiteSpace(connection.Environment))
            {
                throw new InvalidOperationException("Environment is required.");
            }

            if (connection.FullscriptConnectionId.HasValue &&
                GetById(connection.FullscriptConnectionId.Value) != null)
            {
                throw new InvalidOperationException("Fullscript connection already exists.");
            }

            if (!connection.FullscriptConnectionId.HasValue || connection.FullscriptConnectionId.Value <= 0)
            {
                connection.FullscriptConnectionId = _nextId;
                _nextId += 1;
            }
            else if (connection.FullscriptConnectionId.Value >= _nextId)
            {
                _nextId = connection.FullscriptConnectionId.Value + 1;
            }

            if (connection.FullscriptConnectionGuid == Guid.Empty)
            {
                connection.FullscriptConnectionGuid = Guid.NewGuid();
            }

            if (connection.CreatedDateTime == DateTime.MinValue)
            {
                connection.CreatedDateTime = DateTime.UtcNow;
            }

            _items.Add(connection);

            return connection.FullscriptConnectionId.Value;
        }

        public void Update(FullscriptConnectionDto connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (!connection.FullscriptConnectionId.HasValue)
            {
                throw new InvalidOperationException("FullscriptConnectionId is required for update.");
            }

            var existing = GetById(connection.FullscriptConnectionId.Value);

            if (existing == null)
            {
                throw new InvalidOperationException("Fullscript connection was not found.");
            }

            var index = _items.IndexOf(existing);
            connection.UpdatedDateTime = DateTime.UtcNow;
            _items[index] = connection;
        }

        public void MarkInactive(int fullscriptConnectionId)
        {
            var existing = GetById(fullscriptConnectionId);

            if (existing == null)
            {
                throw new InvalidOperationException("Fullscript connection was not found.");
            }

            existing.Active = false;
            existing.Status = "Inactive";
            existing.UpdatedDateTime = DateTime.UtcNow;
        }

        public void SaveTokenState(FullscriptConnectionDto connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (connection.FullscriptConnectionId.HasValue &&
                GetById(connection.FullscriptConnectionId.Value) != null)
            {
                Update(connection);
                return;
            }

            Insert(connection);
        }

        public IList<FullscriptConnectionDto> GetAll()
        {
            return _items.ToList();
        }
    }
}