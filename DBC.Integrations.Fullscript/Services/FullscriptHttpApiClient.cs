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

        public FullscriptHttpApiClient(
            HttpClient httpClient,
            IOptions<FullscriptApiSettings> settings)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            _httpClient = httpClient;
            _settings = settings.Value ?? new FullscriptApiSettings();
        }

        public FullscriptDispatchResultDto DispatchTreatmentPlan(
            FullscriptTransactionDto transaction)
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

            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript API base URL is not configured."
                };
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiToken))
            {
                return new FullscriptDispatchResultDto
                {
                    Success = false,
                    ExternalReferenceId = null,
                    ErrorMessage = "Fullscript API token is not configured."
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
                        _httpClient.PostAsync("treatment_plans", content).GetAwaiter().GetResult())
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

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(
                _settings.BaseUrl.TrimEnd('/') + "/");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiToken);

            _httpClient.DefaultRequestHeaders.Accept.Clear();

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static object BuildTreatmentPlanRequest(
            FullscriptTransactionDto transaction)
        {
            return new
            {
                external_reference_id =
                    transaction.FullscriptTransactionGuid.ToString(),
                patient_id =
                    transaction.FullscriptPatientId,
                practitioner_id =
                    transaction.FullscriptPractitionerId,
                items = new[]
                {
                    new
                    {
                        product_id =
                            transaction.FullscriptProductId,
                        variant_id =
                            transaction.FullscriptVariantId,
                        quantity =
                            1
                    }
                }
            };
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

                if (root.TryGetProperty("id", out JsonElement idElement))
                {
                    return idElement.ToString();
                }

                if (root.TryGetProperty("treatment_plan_id", out JsonElement treatmentPlanIdElement))
                {
                    return treatmentPlanIdElement.ToString();
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