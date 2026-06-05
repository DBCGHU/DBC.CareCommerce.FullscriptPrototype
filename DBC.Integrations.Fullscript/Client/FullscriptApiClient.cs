using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DBC.Integrations.Fullscript.Configuration;

namespace DBC.Integrations.Fullscript.Client
{
    public class FullscriptApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly FullscriptConfiguration _configuration;

        public FullscriptApiClient(FullscriptConfiguration configuration)
            : this(configuration, new HttpClient())
        {
        }

        public FullscriptApiClient(FullscriptConfiguration configuration, HttpClient httpClient)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (httpClient == null)
            {
                throw new ArgumentNullException("httpClient");
            }

            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<string> GetClinicRawJsonAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
            }

            var requestUrl = BuildApiUrl("/api/clinic");

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript API request failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return responseText;
                }
            }
        }

        private string BuildApiUrl(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("API path is required.", "path");
            }

            var baseUrl = _configuration.ApiBaseUrl;

            if (baseUrl.EndsWith("/"))
            {
                baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);
            }

            if (!path.StartsWith("/"))
            {
                path = "/" + path;
            }

            return baseUrl + path;
        }

        public async Task<string> SearchProductsRawJsonAsync(string accessToken, string query)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Product search query is required.", "query");
            }

            var requestUrl = BuildApiUrl("/api/catalog/search/products?query=" + Uri.EscapeDataString(query));

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript product search failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return responseText;
                }
            }
        }

        public async Task<string> GetProductsRawJsonAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
            }

            var requestUrl = BuildApiUrl("/api/catalog/products");

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript catalog products request failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return responseText;
                }
            }
        }

        public async Task<string> GetPatientsRawJsonAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
            }

            var requestUrl = BuildApiUrl("/api/clinic/patients");

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript patients request failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return responseText;
                }
            }
        }

        public async Task<string> SearchPatientsRawJsonAsync(string accessToken, string query)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Patient search query is required.", "query");
            }

            var requestUrl = BuildApiUrl("/api/clinic/search/patients?query=" + Uri.EscapeDataString(query) + "&patient_type=all");

            var json = "{\"query\":\"" + EscapeJson(query) + "\"}";



            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript patient search failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return responseText;
                }
            }
        }

        private static string EscapeJson(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        public async Task<string> GetPatientRawJsonAsync(string accessToken, string patientId)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
            }

            if (string.IsNullOrWhiteSpace(patientId))
            {
                throw new ArgumentException("Patient ID is required.", "patientId");
            }

            var requestUrl = BuildApiUrl("/api/clinic/patients/" + Uri.EscapeDataString(patientId));

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript patient request failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return responseText;
                }
            }
        }

        public async Task<string> SearchPatientsByFieldsRawJsonAsync(string accessToken, string firstName, string lastName, string email)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
            }

            if (string.IsNullOrWhiteSpace(firstName) &&
                string.IsNullOrWhiteSpace(lastName) &&
                string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("At least one patient search field is required.");
            }

            var queryParts = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                queryParts.Add("first_name=" + Uri.EscapeDataString(firstName));
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                queryParts.Add("last_name=" + Uri.EscapeDataString(lastName));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                queryParts.Add("email=" + Uri.EscapeDataString(email));
            }

            queryParts.Add("patient_type=all");

            var requestUrl = BuildApiUrl("/api/clinic/search/patients?" + string.Join("&", queryParts));

            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript patient field search failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return responseText;
                }
            }
        }

        public async Task<string> CreatePatientRawJsonAsync(string accessToken, string firstName, string lastName, string email, string metadataId)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token is required.", "accessToken");
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

            var requestUrl = BuildApiUrl("/api/clinic/patients");

            var json =
                "{" +
                "\"first_name\":\"" + EscapeJson(firstName) + "\"," +
                "\"last_name\":\"" + EscapeJson(lastName) + "\"," +
                "\"email\":\"" + EscapeJson(email) + "\"";

            if (!string.IsNullOrWhiteSpace(metadataId))
            {
                json +=
                    "," +
                    "\"metadata\":{" +
                    "\"id\":\"" + EscapeJson(metadataId) + "\"" +
                    "}";
            }

            json += "}";

            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            "Fullscript create patient request failed. Status: " +
                            ((int)response.StatusCode).ToString() +
                            " " +
                            response.ReasonPhrase +
                            Environment.NewLine +
                            responseText);
                    }

                    return responseText;
                }
            }
        }


    }
}