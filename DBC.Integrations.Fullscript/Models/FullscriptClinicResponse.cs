using System;
using System.Text.Json;

namespace DBC.Integrations.Fullscript.Models
{
    public class FullscriptClinicResponse
    {
        public string ClinicId { get; set; }

        public string Name { get; set; }

        public int? PatientCount { get; set; }

        public int? PractitionerCount { get; set; }

        public int? Discount { get; set; }

        public string DispensaryUrl { get; set; }

        public string IntegrationId { get; set; }

        public DateTime? IntegrationActivatedAt { get; set; }

        public string MarginType { get; set; }

        public bool HasClinicId()
        {
            return !string.IsNullOrWhiteSpace(ClinicId);
        }
    }

    public static class FullscriptClinicResponseParser
    {
        public static FullscriptClinicResponse Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Clinic response JSON is required.", "json");
            }

            using (var document = JsonDocument.Parse(json))
            {
                var root = document.RootElement;

                JsonElement clinicElement;

                if (root.TryGetProperty("clinic", out clinicElement))
                {
                    return ParseClinicElement(clinicElement);
                }

                return ParseClinicElement(root);
            }
        }

        private static FullscriptClinicResponse ParseClinicElement(JsonElement clinicElement)
        {
            return new FullscriptClinicResponse
            {
                ClinicId = GetString(clinicElement, "id"),
                Name = GetString(clinicElement, "name"),
                PatientCount = GetNullableInt(clinicElement, "patient_count"),
                PractitionerCount = GetNullableInt(clinicElement, "practitioner_count"),
                Discount = GetNullableInt(clinicElement, "discount"),
                DispensaryUrl = GetString(clinicElement, "dispensary_url"),
                IntegrationId = GetString(clinicElement, "integration_id"),
                IntegrationActivatedAt = GetNullableDateTime(clinicElement, "integration_activated_at"),
                MarginType = GetString(clinicElement, "margin_type")
            };
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

        private static int? GetNullableInt(JsonElement root, string propertyName)
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

            if (value.ValueKind == JsonValueKind.Number)
            {
                return value.GetInt32();
            }

            int parsed;

            if (int.TryParse(value.GetString(), out parsed))
            {
                return parsed;
            }

            return null;
        }

        private static DateTime? GetNullableDateTime(JsonElement root, string propertyName)
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

            DateTime parsed;

            if (DateTime.TryParse(value.GetString(), out parsed))
            {
                return parsed.ToUniversalTime();
            }

            return null;
        }
    }
}