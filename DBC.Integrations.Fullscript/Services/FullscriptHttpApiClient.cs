using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Services;
using DBC.Integrations.Fullscript.Configuration;
using Microsoft.Extensions.Options;

namespace DBC.Integrations.Fullscript.Services
{
    public sealed class FullscriptHttpApiClient : IFullscriptApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly FullscriptApiSettings _settings;
        private readonly IFullscriptOAuthTokenProvider _oauthTokenProvider;

        public FullscriptHttpApiClient(
            HttpClient httpClient,
            IOptions<FullscriptApiSettings> settings,
            IFullscriptOAuthTokenProvider oauthTokenProvider)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (oauthTokenProvider == null)
            {
                throw new ArgumentNullException("oauthTokenProvider");
            }

            _httpClient = httpClient;
            _settings = settings.Value ?? new FullscriptApiSettings();
            _oauthTokenProvider = oauthTokenProvider;
        }

        public FullscriptDispatchResultDto DispatchTreatmentPlan(
            FullscriptTransactionDto transaction)
        {
            FullscriptDispatchResultDto configurationValidationResult =
                ValidateConfiguration();

            if (!configurationValidationResult.Success)
            {
                return configurationValidationResult;
            }

            if (transaction == null ||
                !transaction.FullscriptTransactionId.HasValue)
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript transaction ID is required before dispatching."
                };
            }

            try
            {
                ConfigureHttpClient();

                object requestBody = BuildTreatmentPlanRequest(transaction);

                string json =
                    JsonSerializer.Serialize(requestBody);

                using (StringContent content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"))
                {
                    using (HttpResponseMessage response =
                        _httpClient.PostAsync(BuildTreatmentPlanEndpoint(transaction), content).GetAwaiter().GetResult())
                    {
                        string responseBody =
                            response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        if (!response.IsSuccessStatusCode)
                        {
                            return new FullscriptDispatchResultDto
                            {
                                Success = false,
                                ExternalReferenceId = null,
                                ErrorMessage =
                                    "Fullscript API request failed with status " +
                                    ((int)response.StatusCode).ToString() +
                                    ": " +
                                    responseBody
                            };
                        }

                        string externalReferenceId =
                            ExtractExternalReferenceId(responseBody);

                        if (string.IsNullOrWhiteSpace(externalReferenceId))
                        {
                            return new FullscriptDispatchResultDto
                            {
                                Success = false,
                                ExternalReferenceId = null,
                                ErrorMessage =
                                    "Fullscript API response did not include a treatment plan identifier."
                            };
                        }

                        return new FullscriptDispatchResultDto
                        {
                            Success = true,
                            ExternalReferenceId = externalReferenceId,
                            ErrorMessage = null
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript API request failed: " + ex.Message
                };
            }
        }

        public FullscriptPatientCreateResultDto CreatePatient(
            FullscriptPatientCreateRequestDto request)
        {
            FullscriptDispatchResultDto configurationValidationResult =
                ValidateConfiguration();

            if (!configurationValidationResult.Success)
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = configurationValidationResult.ErrorMessage
                };
            }

            FullscriptPatientCreateResultDto requestValidationResult =
                ValidatePatientCreateRequest(request);

            if (!requestValidationResult.Success)
            {
                return requestValidationResult;
            }

            try
            {
                ConfigureHttpClient();

                object requestBody = BuildPatientCreateRequest(request);

                string json =
                    JsonSerializer.Serialize(requestBody);

                using (StringContent content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"))
                {
                    using (HttpResponseMessage response =
                        _httpClient.PostAsync("api/clinic/patients", content).GetAwaiter().GetResult())
                    {
                        string responseBody =
                            response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                        if (!response.IsSuccessStatusCode)
                        {
                            return new FullscriptPatientCreateResultDto
                            {
                                Success = false,
                                FullscriptPatientId = null,
                                ErrorMessage =
                                    "Fullscript patient creation request failed with status " +
                                    ((int)response.StatusCode).ToString() +
                                    ": " +
                                    responseBody
                            };
                        }

                        string fullscriptPatientId =
                            ExtractPatientId(responseBody);

                        if (string.IsNullOrWhiteSpace(fullscriptPatientId))
                        {
                            return new FullscriptPatientCreateResultDto
                            {
                                Success = false,
                                FullscriptPatientId = null,
                                ErrorMessage =
                                    "Fullscript patient creation response did not include a patient identifier."
                            };
                        }

                        return new FullscriptPatientCreateResultDto
                        {
                            Success = true,
                            FullscriptPatientId = fullscriptPatientId,
                            ErrorMessage = null
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Fullscript patient creation request failed: " + ex.Message
                };
            }
        }

        private FullscriptDispatchResultDto ValidateConfiguration()
        {
            if (!_settings.Enabled)
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript HTTP API client is not enabled."
                };
            }

            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript API base URL is not configured."
                };
            }

            Uri baseUri;

            if (!Uri.TryCreate(_settings.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out baseUri))
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript API base URL is not a valid absolute URL."
                };
            }

            if (baseUri.Scheme != Uri.UriSchemeHttp &&
                baseUri.Scheme != Uri.UriSchemeHttps)
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript API base URL must use HTTP or HTTPS."
                };
            }

            FullscriptOAuthTokenResult tokenResult =
                _oauthTokenProvider.GetCurrentToken();

            if (tokenResult == null ||
                !tokenResult.Success ||
                !tokenResult.HasAccessToken())
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript OAuth access token is not available. Complete the OAuth flow before using Fullscript HTTP client mode."
                };
            }

            return new FullscriptDispatchResultDto
            {
                Success = true,
                ExternalReferenceId = null,
                ErrorMessage = null
            };
        }

        private void ConfigureHttpClient()
        {
            FullscriptOAuthTokenResult tokenResult =
                _oauthTokenProvider.GetCurrentToken();

            if (tokenResult == null ||
                !tokenResult.Success ||
                !tokenResult.HasAccessToken())
            {
                throw new InvalidOperationException(
                    "Fullscript OAuth access token is not available. Complete the OAuth flow before using Fullscript HTTP client mode.");
            }

            _httpClient.BaseAddress = new Uri(
                _settings.BaseUrl.TrimEnd('/') + "/");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);

            if (!_httpClient.DefaultRequestHeaders.Accept.Contains(
                new MediaTypeWithQualityHeaderValue("application/json")))
            {
                _httpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        private static string BuildTreatmentPlanEndpoint(
            FullscriptTransactionDto transaction)
        {
            return "api/clinic/patients/" +
                Uri.EscapeDataString(transaction.FullscriptPatientId) +
                "/treatment_plans";
        }

        private static object BuildTreatmentPlanRequest(
            FullscriptTransactionDto transaction)
        {
            return new
            {
                practitioner_id =
                    transaction.FullscriptPractitionerId,
                state =
                    "active",
                recommendations = new[]
                {
                    new
                    {
                        variant_id =
                            transaction.FullscriptVariantId,
                        units_to_purchase =
                            "1"
                    }
                },
                metadata = new
                {
                    id =
                        transaction.FullscriptTransactionGuid.ToString()
                }
            };
        }

        private static FullscriptPatientCreateResultDto ValidatePatientCreateRequest(
            FullscriptPatientCreateRequestDto request)
        {
            if (request == null)
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Fullscript patient create request is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Fullscript patient create request email is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.FirstName))
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Fullscript patient create request first name is required."
                };
            }

            if (string.IsNullOrWhiteSpace(request.LastName))
            {
                return new FullscriptPatientCreateResultDto
                {
                    Success = false,
                    FullscriptPatientId = null,
                    ErrorMessage = "Fullscript patient create request last name is required."
                };
            }

            return new FullscriptPatientCreateResultDto
            {
                Success = true,
                FullscriptPatientId = null,
                ErrorMessage = null
            };
        }

        private static object BuildPatientCreateRequest(
            FullscriptPatientCreateRequestDto request)
        {
            return new
            {
                email = request.Email,
                first_name = request.FirstName,
                last_name = request.LastName,
                date_of_birth = FormatDateOfBirth(request.DateOfBirth),
                metadata = new
                {
                    id = request.MetadataId
                }
            };
        }

        private static string FormatDateOfBirth(DateTime? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
            {
                return null;
            }

            return dateOfBirth.Value.ToString("yyyy-MM-dd");
        }

        private static string ExtractExternalReferenceId(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(responseBody))
            {
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("treatment_plan", out JsonElement treatmentPlanElement) &&
                    treatmentPlanElement.TryGetProperty("id", out JsonElement treatmentPlanIdElement))
                {
                    return treatmentPlanIdElement.ToString();
                }

                if (root.TryGetProperty("id", out JsonElement idElement))
                {
                    return idElement.ToString();
                }

                if (root.TryGetProperty("treatment_plan_id", out JsonElement treatmentPlanIdFallbackElement))
                {
                    return treatmentPlanIdFallbackElement.ToString();
                }

                if (root.TryGetProperty("data", out JsonElement dataElement) &&
                    dataElement.TryGetProperty("id", out JsonElement dataIdElement))
                {
                    return dataIdElement.ToString();
                }
            }

            return null;
        }

        private static string ExtractPatientId(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return null;
            }

            using (JsonDocument document = JsonDocument.Parse(responseBody))
            {
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("patient", out JsonElement patientElement) &&
                    patientElement.TryGetProperty("id", out JsonElement patientIdElement))
                {
                    return patientIdElement.ToString();
                }

                if (root.TryGetProperty("id", out JsonElement idElement))
                {
                    return idElement.ToString();
                }

                if (root.TryGetProperty("patient_id", out JsonElement patientIdFallbackElement))
                {
                    return patientIdFallbackElement.ToString();
                }

                if (root.TryGetProperty("data", out JsonElement dataElement) &&
                    dataElement.TryGetProperty("id", out JsonElement dataIdElement))
                {
                    return dataIdElement.ToString();
                }
            }

            return null;
        }
    }
}