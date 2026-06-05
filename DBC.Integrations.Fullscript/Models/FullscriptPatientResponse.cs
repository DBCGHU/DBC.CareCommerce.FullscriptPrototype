using System;
using System.Text.Json;

namespace DBC.Integrations.Fullscript.Models
{
    public class FullscriptPatientResponse
    {
        public string PatientId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool? Archived { get; set; }

        public string MetadataId { get; set; }

        public bool HasPatientId()
        {
            return !string.IsNullOrWhiteSpace(PatientId);
        }
    }

    public static class FullscriptPatientResponseParser
    {
        public static FullscriptPatientResponse ParseSinglePatientResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Patient response JSON is required.", "json");
            }

            using (var document = JsonDocument.Parse(json))
            {
                var root = document.RootElement;

                JsonElement patientElement;

                if (root.TryGetProperty("patient", out patientElement))
                {
                    return ParsePatientElement(patientElement);
                }

                return ParsePatientElement(root);
            }
        }

        private static FullscriptPatientResponse ParsePatientElement(JsonElement patientElement)
        {
            var response = new FullscriptPatientResponse
            {
                PatientId = GetString(patientElement, "id"),
                FirstName = GetString(patientElement, "first_name"),
                LastName = GetString(patientElement, "last_name"),
                Email = GetString(patientElement, "email"),
                Archived = GetNullableBool(patientElement, "archived")
            };

            JsonElement metadataElement;

            if (patientElement.TryGetProperty("metadata", out metadataElement))
            {
                response.MetadataId = GetString(metadataElement, "id");
            }

            return response;
        }

        private static string GetString(JsonElement root, string propertyName)
        {
            JsonElement value;

            if (!root.TryGetProperty(propertyName, out value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return value.GetString();
        }

        private static bool? GetNullableBool(JsonElement root, string propertyName)
        {
            JsonElement value;

            if (!root.TryGetProperty(propertyName, out value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.True)
            {
                return true;
            }

            if (value.ValueKind == JsonValueKind.False)
            {
                return false;
            }

            bool parsed;

            if (bool.TryParse(value.GetString(), out parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
