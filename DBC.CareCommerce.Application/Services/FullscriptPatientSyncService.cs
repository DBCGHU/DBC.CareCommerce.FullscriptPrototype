using System;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Contracts.Services;

namespace DBC.CareCommerce.Application.Services
{
    public sealed class FullscriptPatientSyncService
    {
        private readonly IPatientProfileRepository _patientProfileRepository;
        private readonly IFullscriptApiClient _fullscriptApiClient;

        public FullscriptPatientSyncService(
            IPatientProfileRepository patientProfileRepository,
            IFullscriptApiClient fullscriptApiClient)
        {
            if (patientProfileRepository == null)
            {
                throw new ArgumentNullException("patientProfileRepository");
            }

            if (fullscriptApiClient == null)
            {
                throw new ArgumentNullException("fullscriptApiClient");
            }

            _patientProfileRepository = patientProfileRepository;
            _fullscriptApiClient = fullscriptApiClient;
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

            return _fullscriptApiClient.CreatePatient(request);
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
    }
}