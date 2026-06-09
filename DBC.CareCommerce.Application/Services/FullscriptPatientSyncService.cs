using System;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Contracts.Services;
using DBC.CareCommerce.Data.Repositories;

namespace DBC.CareCommerce.Application.Services
{
    public sealed class FullscriptPatientSyncService
    {
        private readonly IPatientProfileRepository _patientProfileRepository;
        private readonly IFullscriptApiClient _fullscriptApiClient;
        private readonly IFullscriptPatientMapRepository _fullscriptPatientMapRepository;

        public FullscriptPatientSyncService(
            IPatientProfileRepository patientProfileRepository,
            IFullscriptApiClient fullscriptApiClient,
            IFullscriptPatientMapRepository fullscriptPatientMapRepository)
        {
            if (patientProfileRepository == null)
            {
                throw new ArgumentNullException("patientProfileRepository");
            }

            if (fullscriptApiClient == null)
            {
                throw new ArgumentNullException("fullscriptApiClient");
            }

            if (fullscriptPatientMapRepository == null)
            {
                throw new ArgumentNullException("fullscriptPatientMapRepository");
            }

            _patientProfileRepository = patientProfileRepository;
            _fullscriptApiClient = fullscriptApiClient;
            _fullscriptPatientMapRepository = fullscriptPatientMapRepository;
        }

        public FullscriptPatientCreateResultDto CreatePatientForLocalPatient(
            int patientId)
        {
            if (patientId <= 0)
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Patient ID is required before creating a Fullscript patient."
                };
            }

            FullscriptPatientProfileDto patientProfile =
                _patientProfileRepository.GetByPatientId(patientId);

            FullscriptPatientCreateResultDto validationResult =
                ValidatePatientProfile(patientProfile);

            if (!validationResult.Success)
            {
                return validationResult;
            }

            FullscriptPatientCreateRequestDto request =
                BuildCreateRequest(patientProfile);

            FullscriptPatientCreateResultDto createResult =
                _fullscriptApiClient.CreatePatient(request);

            if (!createResult.Success)
            {
                return createResult;
            }

            if (string.IsNullOrWhiteSpace(createResult.FullscriptPatientId))
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Fullscript patient creation succeeded but did not return a patient ID."
                };
            }

            SavePatientMap(patientProfile, createResult.FullscriptPatientId, request.MetadataId);

            return createResult;
        }

        private static FullscriptPatientCreateResultDto ValidatePatientProfile(
            FullscriptPatientProfileDto patientProfile)
        {
            if (patientProfile == null)
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Local patient profile was not found."
                };
            }

            if (patientProfile.PatientId <= 0)
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Local patient profile does not include a valid patient ID."
                };
            }

            if (string.IsNullOrWhiteSpace(patientProfile.Email))
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Local patient profile does not include an email address."
                };
            }

            if (string.IsNullOrWhiteSpace(patientProfile.FirstName))
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Local patient profile does not include a first name."
                };
            }

            if (string.IsNullOrWhiteSpace(patientProfile.LastName))
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Local patient profile does not include a last name."
                };
            }

            return new FullscriptPatientCreateResultDto
            {
                Success = true,
                FullscriptPatientId = null,
                ErrorMessage = null
            };
        }

        private static FullscriptPatientCreateRequestDto BuildCreateRequest(
            FullscriptPatientProfileDto patientProfile)
        {
            return new FullscriptPatientCreateRequestDto
            {
                PatientId = patientProfile.PatientId,
                Email = patientProfile.Email,
                FirstName = patientProfile.FirstName,
                LastName = patientProfile.LastName,
                DateOfBirth = patientProfile.DateOfBirth,
                MetadataId = "dbc-patient-" + patientProfile.PatientId
            };
        }

        private void SavePatientMap(
            FullscriptPatientProfileDto patientProfile,
            string fullscriptPatientId,
            string metadataId)
        {
            FullscriptPatientMapDto map = new FullscriptPatientMapDto
            {
                PatientId = patientProfile.PatientId,
                FullscriptPatientId = fullscriptPatientId,
                FullscriptMetadataId = metadataId,
                FullscriptEmail = patientProfile.Email,
                FullscriptFirstName = patientProfile.FirstName,
                FullscriptLastName = patientProfile.LastName,
                Environment = "UsSandbox",
                ClinicId = null,
                LastSyncedDateTime = DateTime.UtcNow,
                Active = true,
                UpdatedDateTime = DateTime.UtcNow
            };

            _fullscriptPatientMapRepository.Insert(map);
        }
    }
}