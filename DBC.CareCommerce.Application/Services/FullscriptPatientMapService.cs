using System;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;
using DBC.Integrations.Fullscript.Configuration;
using Microsoft.Extensions.Options;

namespace DBC.CareCommerce.Application.Services
{
    public sealed class FullscriptPatientMapService
    {
        private readonly IFullscriptPatientMapRepository _fullscriptPatientMapRepository;
        private readonly FullscriptApiSettings _settings;

        public FullscriptPatientMapService(
            IFullscriptPatientMapRepository fullscriptPatientMapRepository,
            IOptions<FullscriptApiSettings> settings)
        {
            if (fullscriptPatientMapRepository == null)
            {
                throw new ArgumentNullException("fullscriptPatientMapRepository");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            _fullscriptPatientMapRepository = fullscriptPatientMapRepository;
            _settings = settings.Value ?? new FullscriptApiSettings();
        }

        public string ResolveMappedFullscriptPatientId(int patientId)
        {
            if (patientId <= 0)
            {
                return null;
            }

            FullscriptPatientMapDto map =
                _fullscriptPatientMapRepository.GetByPatientId(
                    patientId,
                    ResolveEnvironment(),
                    null);

            if (map == null || !map.HasFullscriptPatientId())
            {
                return null;
            }

            return map.FullscriptPatientId;
        }

        private string ResolveEnvironment()
        {
            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                return "UsSandbox";
            }

            string baseUrl = _settings.BaseUrl.ToLowerInvariant();

            if (baseUrl.Contains("api-ca-snd"))
            {
                return "CaSandbox";
            }

            if (baseUrl.Contains("api-us.fullscript"))
            {
                return "UsProduction";
            }

            if (baseUrl.Contains("api-ca.fullscript"))
            {
                return "CaProduction";
            }

            return "UsSandbox";
        }
    }
}