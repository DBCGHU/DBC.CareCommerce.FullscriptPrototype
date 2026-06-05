using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Data.Repositories
{
    public interface IFullscriptConnectionRepository
    {
        FullscriptConnectionDto GetById(int fullscriptConnectionId);

        FullscriptConnectionDto GetActiveByEnvironmentAndClinic(string environment, string clinicId);

        IList<FullscriptConnectionDto> GetActiveByEnvironment(string environment);

        int Insert(FullscriptConnectionDto connection);

        void Update(FullscriptConnectionDto connection);

        void MarkInactive(int fullscriptConnectionId);

        void SaveTokenState(FullscriptConnectionDto connection);
    }
}