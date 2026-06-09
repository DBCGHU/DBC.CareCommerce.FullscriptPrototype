using System;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;

namespace DBC.CareCommerce.Application.Services
{
    public sealed class FullscriptPatientMapService
    {
        private readonly IFullscriptPatientMapRepository _fullscriptPatientMapRepository;

        public FullscriptPatientMapService(
            IFullscriptPatientMapRepository fullscriptPatientMapRepository)
        {
            if (fullscriptPatientMapRepository == null)
            {
                throw new ArgumentNullException("fullscriptPatientMapRepository");
            }

            _fullscriptPatientMapRepository = fullscriptPatientMapRepository;
        }

        public string ResolveMappedFullscriptPatientId(
            int patientId,
            string environment,
            string clinicId)
        {
            if (patientId <= 0)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(environment))
            {
                environment = "UsSandbox";
            }

            FullscriptPatientMapDto map =
                _fullscriptPatientMapRepository.GetByPatientId(
                    patientId,
                    environment,
                    clinicId);

            if (map == null || !map.HasFullscriptPatientId())
            {
                return null;
            }

            return map.FullscriptPatientId;
        }
    }
}