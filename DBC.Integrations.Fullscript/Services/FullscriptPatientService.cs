using System;
using System.Threading.Tasks;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;
using DBC.Integrations.Fullscript.Client;
using DBC.Integrations.Fullscript.Models;

namespace DBC.Integrations.Fullscript.Services
{
    public class FullscriptPatientService
    {
        private readonly FullscriptApiClient _apiClient;
        private readonly IFullscriptPatientMapRepository _patientMapRepository;

        public FullscriptPatientService(
            FullscriptApiClient apiClient,
            IFullscriptPatientMapRepository patientMapRepository)
        {
            if (apiClient == null)
            {
                throw new ArgumentNullException("apiClient");
            }

            if (patientMapRepository == null)
            {
                throw new ArgumentNullException("patientMapRepository");
            }

            _apiClient = apiClient;
            _patientMapRepository = patientMapRepository;
        }

        public async Task<FullscriptPatientMapDto> GetOrCreatePatientMapAsync(
            string accessToken,
            int dbcPatientId,
            string firstName,
            string lastName,
            string email,
            string environment,
            string clinicId)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
            }

            if (dbcPatientId <= 0)
            {
                throw new ArgumentException("DBC PatientID is required.", "dbcPatientId");
            }

            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("First name is required.", "firstName");
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException("Last name is required.", "lastName");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.", "email");
            }

            if (string.IsNullOrWhiteSpace(environment))
            {
                environment = "UsSandbox";
            }

            var existingMap = _patientMapRepository.GetByPatientId(
                dbcPatientId,
                environment,
                clinicId);

            if (existingMap != null && existingMap.HasFullscriptPatientId())
            {
                return existingMap;
            }

            var metadataId = BuildMetadataId(dbcPatientId);

            var createPatientJson = await _apiClient.CreatePatientRawJsonAsync(
                accessToken,
                firstName,
                lastName,
                email,
                metadataId).ConfigureAwait(false);

            var parsedPatient = FullscriptPatientResponseParser.ParseSinglePatientResponse(createPatientJson);

            if (parsedPatient == null || !parsedPatient.HasPatientId())
            {
                throw new InvalidOperationException("Fullscript create patient response did not include a patient ID.");
            }

            var map = new FullscriptPatientMapDto
            {
                PatientId = dbcPatientId,
                FullscriptPatientId = parsedPatient.PatientId,
                FullscriptMetadataId = parsedPatient.MetadataId,
                FullscriptEmail = parsedPatient.Email,
                FullscriptFirstName = parsedPatient.FirstName,
                FullscriptLastName = parsedPatient.LastName,
                Environment = environment,
                ClinicId = clinicId,
                LastSyncedDateTime = DateTime.UtcNow
            };

            var mapId = _patientMapRepository.Insert(map);
            map.FullscriptPatientMapId = mapId;

            return map;
        }

        public FullscriptPatientMapDto GetExistingPatientMap(
            int dbcPatientId,
            string environment,
            string clinicId)
        {
            if (dbcPatientId <= 0)
            {
                throw new ArgumentException("DBC PatientID is required.", "dbcPatientId");
            }

            if (string.IsNullOrWhiteSpace(environment))
            {
                environment = "UsSandbox";
            }

            return _patientMapRepository.GetByPatientId(
                dbcPatientId,
                environment,
                clinicId);
        }

        public static string BuildMetadataId(int dbcPatientId)
        {
            if (dbcPatientId <= 0)
            {
                throw new ArgumentException("DBC PatientID is required.", "dbcPatientId");
            }

            return "DBC-PATIENT-" + dbcPatientId;
        }
    }
}