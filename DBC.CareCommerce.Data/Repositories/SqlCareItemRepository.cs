using System;
using System.Collections.Generic;
using System.Data;
using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.DataAccess;
using Microsoft.Data.SqlClient;

namespace DBC.CareCommerce.Data.Repositories
{
    public sealed class SqlCareItemRepository : ICareItemRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public SqlCareItemRepository(SqlConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            _connectionFactory = connectionFactory;
        }

        public CareItemDto GetById(int careItemId)
        {
            const string sql = @"
SELECT
    CareItemID,
    CareItemGuid,
    PatientID,
    PatientCaseID,
    VisitID,
    ProviderID,
    CatalogItemID,
    SourceSystem,
    SourceEntityType,
    SourceEntityID,
    TreatmentID,
    MedicationID,
    SupplementRecordID,
    PostingID,
    CareItemType,
    ClinicalStatus,
    FulfillmentSource,
    BillingIntent,
    InventoryIntent,
    QuantityRecommended,
    QuantityDispensed,
    DosageAmount,
    DosageFrequency,
    DosageDuration,
    DosageFormat,
    TakeWith,
    Instructions,
    NarrativeText,
    ProductID,
    FeeID,
    FullscriptVariantID,
    RequiresPatientCase,
    CreatedByUserID,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.CareItem
WHERE CareItemID = @CareItemID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@CareItemID", SqlDbType.Int).Value = careItemId;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return MapReader(reader);
                    }
                }
            }
        }

        public IList<CareItemDto> GetByPatientCase(int patientId, int patientCaseId)
        {
            List<CareItemDto> items = new List<CareItemDto>();

            const string sql = @"
SELECT
    CareItemID,
    CareItemGuid,
    PatientID,
    PatientCaseID,
    VisitID,
    ProviderID,
    CatalogItemID,
    SourceSystem,
    SourceEntityType,
    SourceEntityID,
    TreatmentID,
    MedicationID,
    SupplementRecordID,
    PostingID,
    CareItemType,
    ClinicalStatus,
    FulfillmentSource,
    BillingIntent,
    InventoryIntent,
    QuantityRecommended,
    QuantityDispensed,
    DosageAmount,
    DosageFrequency,
    DosageDuration,
    DosageFormat,
    TakeWith,
    Instructions,
    NarrativeText,
    ProductID,
    FeeID,
    FullscriptVariantID,
    RequiresPatientCase,
    CreatedByUserID,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.CareItem
WHERE
    Active = 1
    AND PatientID = @PatientID
    AND PatientCaseID = @PatientCaseID
ORDER BY CareItemID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@PatientID", SqlDbType.Int).Value = patientId;
                    command.Parameters.Add("@PatientCaseID", SqlDbType.Int).Value = patientCaseId;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(MapReader(reader));
                        }
                    }
                }
            }

            return items;
        }

        public IList<CareItemDto> GetByVisit(int visitId)
        {
            List<CareItemDto> items = new List<CareItemDto>();

            const string sql = @"
SELECT
    CareItemID,
    CareItemGuid,
    PatientID,
    PatientCaseID,
    VisitID,
    ProviderID,
    CatalogItemID,
    SourceSystem,
    SourceEntityType,
    SourceEntityID,
    TreatmentID,
    MedicationID,
    SupplementRecordID,
    PostingID,
    CareItemType,
    ClinicalStatus,
    FulfillmentSource,
    BillingIntent,
    InventoryIntent,
    QuantityRecommended,
    QuantityDispensed,
    DosageAmount,
    DosageFrequency,
    DosageDuration,
    DosageFormat,
    TakeWith,
    Instructions,
    NarrativeText,
    ProductID,
    FeeID,
    FullscriptVariantID,
    RequiresPatientCase,
    CreatedByUserID,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.CareItem
WHERE
    Active = 1
    AND VisitID = @VisitID
ORDER BY CareItemID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@VisitID", SqlDbType.Int).Value = visitId;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(MapReader(reader));
                        }
                    }
                }
            }

            return items;
        }

        public int Insert(CareItemDto item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (item.PatientId <= 0)
            {
                throw new InvalidOperationException("PatientId is required.");
            }

            if (item.CareItemGuid == Guid.Empty)
            {
                item.CareItemGuid = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(item.SourceSystem))
            {
                item.SourceSystem = "Unknown";
            }

            if (string.IsNullOrWhiteSpace(item.CareItemType))
            {
                item.CareItemType = "Unknown";
            }

            if (item.CreatedDateTime == DateTime.MinValue)
            {
                item.CreatedDateTime = DateTime.UtcNow;
            }

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
INSERT INTO dbo.CareItem
(
    CareItemGuid,
    PatientID,
    PatientCaseID,
    VisitID,
    ProviderID,
    CatalogItemID,
    SourceSystem,
    SourceEntityType,
    SourceEntityID,
    TreatmentID,
    MedicationID,
    SupplementRecordID,
    PostingID,
    CareItemType,
    ClinicalStatus,
    FulfillmentSource,
    BillingIntent,
    InventoryIntent,
    QuantityRecommended,
    QuantityDispensed,
    DosageAmount,
    DosageFrequency,
    DosageDuration,
    DosageFormat,
    TakeWith,
    Instructions,
    NarrativeText,
    ProductID,
    FeeID,
    FullscriptVariantID,
    RequiresPatientCase,
    CreatedByUserID,
    CreatedDateTime,
    UpdatedDateTime,
    Active
)
VALUES
(
    @CareItemGuid,
    @PatientID,
    @PatientCaseID,
    @VisitID,
    @ProviderID,
    @CatalogItemID,
    @SourceSystem,
    @SourceEntityType,
    @SourceEntityID,
    @TreatmentID,
    @MedicationID,
    @SupplementRecordID,
    @PostingID,
    @CareItemType,
    @ClinicalStatus,
    @FulfillmentSource,
    @BillingIntent,
    @InventoryIntent,
    @QuantityRecommended,
    @QuantityDispensed,
    @DosageAmount,
    @DosageFrequency,
    @DosageDuration,
    @DosageFormat,
    @TakeWith,
    @Instructions,
    @NarrativeText,
    @ProductID,
    @FeeID,
    @FullscriptVariantID,
    @RequiresPatientCase,
    @CreatedByUserID,
    @CreatedDateTime,
    @UpdatedDateTime,
    @Active
);

SELECT CONVERT(int, SCOPE_IDENTITY());";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, item);

                    int newId = Convert.ToInt32(command.ExecuteScalar());
                    item.CareItemId = newId;

                    return newId;
                }
            }
        }

        public void Update(CareItemDto item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (!item.CareItemId.HasValue)
            {
                throw new InvalidOperationException("CareItemId is required for update.");
            }

            if (item.PatientId <= 0)
            {
                throw new InvalidOperationException("PatientId is required.");
            }

            item.UpdatedDateTime = DateTime.UtcNow;

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
UPDATE dbo.CareItem
SET
    CareItemGuid = @CareItemGuid,
    PatientID = @PatientID,
    PatientCaseID = @PatientCaseID,
    VisitID = @VisitID,
    ProviderID = @ProviderID,
    CatalogItemID = @CatalogItemID,
    SourceSystem = @SourceSystem,
    SourceEntityType = @SourceEntityType,
    SourceEntityID = @SourceEntityID,
    TreatmentID = @TreatmentID,
    MedicationID = @MedicationID,
    SupplementRecordID = @SupplementRecordID,
    PostingID = @PostingID,
    CareItemType = @CareItemType,
    ClinicalStatus = @ClinicalStatus,
    FulfillmentSource = @FulfillmentSource,
    BillingIntent = @BillingIntent,
    InventoryIntent = @InventoryIntent,
    QuantityRecommended = @QuantityRecommended,
    QuantityDispensed = @QuantityDispensed,
    DosageAmount = @DosageAmount,
    DosageFrequency = @DosageFrequency,
    DosageDuration = @DosageDuration,
    DosageFormat = @DosageFormat,
    TakeWith = @TakeWith,
    Instructions = @Instructions,
    NarrativeText = @NarrativeText,
    ProductID = @ProductID,
    FeeID = @FeeID,
    FullscriptVariantID = @FullscriptVariantID,
    RequiresPatientCase = @RequiresPatientCase,
    CreatedByUserID = @CreatedByUserID,
    CreatedDateTime = @CreatedDateTime,
    UpdatedDateTime = @UpdatedDateTime,
    Active = @Active
WHERE CareItemID = @CareItemID;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, item);
                    command.Parameters.Add("@CareItemID", SqlDbType.Int).Value = item.CareItemId.Value;

                    int affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("Care item was not found.");
                    }
                }
            }
        }

        private static CareItemDto MapReader(SqlDataReader reader)
        {
            return new CareItemDto
            {
                CareItemId = GetNullableInt(reader, "CareItemID"),
                CareItemGuid = GetGuid(reader, "CareItemGuid"),
                PatientId = GetInt(reader, "PatientID"),
                PatientCaseId = GetNullableInt(reader, "PatientCaseID"),
                VisitId = GetNullableInt(reader, "VisitID"),
                ProviderId = GetNullableInt(reader, "ProviderID"),
                CatalogItemId = GetNullableInt(reader, "CatalogItemID"),
                SourceSystem = GetString(reader, "SourceSystem"),
                SourceEntityType = GetNullableString(reader, "SourceEntityType"),
                SourceEntityId = GetNullableInt(reader, "SourceEntityID"),
                TreatmentId = GetNullableInt(reader, "TreatmentID"),
                MedicationId = GetNullableInt(reader, "MedicationID"),
                SupplementRecordId = GetNullableInt(reader, "SupplementRecordID"),
                PostingId = GetNullableInt(reader, "PostingID"),
                CareItemType = GetString(reader, "CareItemType"),
                ClinicalStatus = ParseCareItemStatus(GetString(reader, "ClinicalStatus")),
                FulfillmentSource = ParseFulfillmentSource(GetString(reader, "FulfillmentSource")),
                BillingIntent = ParseBillingAction(GetString(reader, "BillingIntent")),
                InventoryIntent = ParseInventoryAction(GetString(reader, "InventoryIntent")),
                QuantityRecommended = GetNullableDecimal(reader, "QuantityRecommended"),
                QuantityDispensed = GetNullableDecimal(reader, "QuantityDispensed"),
                DosageAmount = GetNullableString(reader, "DosageAmount"),
                DosageFrequency = GetNullableString(reader, "DosageFrequency"),
                DosageDuration = GetNullableString(reader, "DosageDuration"),
                DosageFormat = GetNullableString(reader, "DosageFormat"),
                TakeWith = GetNullableString(reader, "TakeWith"),
                Instructions = GetNullableString(reader, "Instructions"),
                NarrativeText = GetNullableString(reader, "NarrativeText"),
                ProductId = GetNullableInt(reader, "ProductID"),
                FeeId = GetNullableInt(reader, "FeeID"),
                FullscriptVariantId = GetNullableString(reader, "FullscriptVariantID"),
                RequiresPatientCase = GetBool(reader, "RequiresPatientCase"),
                CreatedByUserId = GetNullableInt(reader, "CreatedByUserID"),
                CreatedDateTime = GetDateTime(reader, "CreatedDateTime"),
                UpdatedDateTime = GetNullableDateTime(reader, "UpdatedDateTime"),
                Active = GetBool(reader, "Active")
            };
        }

        private static void AddParameters(SqlCommand command, CareItemDto item)
        {
            command.Parameters.Add("@CareItemGuid", SqlDbType.UniqueIdentifier).Value = item.CareItemGuid;
            command.Parameters.Add("@PatientID", SqlDbType.Int).Value = item.PatientId;

            AddNullableInt(command, "@PatientCaseID", item.PatientCaseId);
            AddNullableInt(command, "@VisitID", item.VisitId);
            AddNullableInt(command, "@ProviderID", item.ProviderId);
            AddNullableInt(command, "@CatalogItemID", item.CatalogItemId);

            AddNVarChar(command, "@SourceSystem", item.SourceSystem, 50);
            AddNullableNVarChar(command, "@SourceEntityType", item.SourceEntityType, 50);
            AddNullableInt(command, "@SourceEntityID", item.SourceEntityId);

            AddNullableInt(command, "@TreatmentID", item.TreatmentId);
            AddNullableInt(command, "@MedicationID", item.MedicationId);
            AddNullableInt(command, "@SupplementRecordID", item.SupplementRecordId);
            AddNullableInt(command, "@PostingID", item.PostingId);

            AddNVarChar(command, "@CareItemType", item.CareItemType, 50);
            AddNVarChar(command, "@ClinicalStatus", item.ClinicalStatus.ToString(), 50);
            AddNVarChar(command, "@FulfillmentSource", item.FulfillmentSource.ToString(), 50);
            AddNVarChar(command, "@BillingIntent", item.BillingIntent.ToString(), 50);
            AddNVarChar(command, "@InventoryIntent", item.InventoryIntent.ToString(), 50);

            AddNullableDecimal(command, "@QuantityRecommended", item.QuantityRecommended, 18, 4);
            AddNullableDecimal(command, "@QuantityDispensed", item.QuantityDispensed, 18, 4);

            AddNullableNVarChar(command, "@DosageAmount", item.DosageAmount, 100);
            AddNullableNVarChar(command, "@DosageFrequency", item.DosageFrequency, 100);
            AddNullableNVarChar(command, "@DosageDuration", item.DosageDuration, 100);
            AddNullableNVarChar(command, "@DosageFormat", item.DosageFormat, 100);
            AddNullableNVarChar(command, "@TakeWith", item.TakeWith, 100);
            AddNullableNVarChar(command, "@Instructions", item.Instructions, -1);
            AddNullableNVarChar(command, "@NarrativeText", item.NarrativeText, -1);

            AddNullableInt(command, "@ProductID", item.ProductId);
            AddNullableInt(command, "@FeeID", item.FeeId);
            AddNullableNVarChar(command, "@FullscriptVariantID", item.FullscriptVariantId, 100);

            AddBool(command, "@RequiresPatientCase", item.RequiresPatientCase);
            AddNullableInt(command, "@CreatedByUserID", item.CreatedByUserId);
            AddDateTime(command, "@CreatedDateTime", item.CreatedDateTime);
            AddNullableDateTime(command, "@UpdatedDateTime", item.UpdatedDateTime);
            AddBool(command, "@Active", item.Active);
        }

        private static CareItemStatus ParseCareItemStatus(string value)
        {
            CareItemStatus parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : CareItemStatus.Draft;
        }

        private static FulfillmentSource ParseFulfillmentSource(string value)
        {
            FulfillmentSource parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : FulfillmentSource.None;
        }

        private static BillingAction ParseBillingAction(string value)
        {
            BillingAction parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : BillingAction.NoBilling;
        }

        private static InventoryAction ParseInventoryAction(string value)
        {
            InventoryAction parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : InventoryAction.None;
        }

        private static void AddNVarChar(SqlCommand command, string parameterName, string value, int size)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.NVarChar, size);
            parameter.Value = value;
        }

        private static void AddNullableNVarChar(SqlCommand command, string parameterName, string value, int size)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.NVarChar, size);
            parameter.Value = string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
        }

        private static void AddNullableInt(SqlCommand command, string parameterName, int? value)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.Int);
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static void AddNullableDecimal(SqlCommand command, string parameterName, decimal? value, byte precision, byte scale)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.Decimal);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static void AddBool(SqlCommand command, string parameterName, bool value)
        {
            command.Parameters.Add(parameterName, SqlDbType.Bit).Value = value;
        }

        private static void AddDateTime(SqlCommand command, string parameterName, DateTime value)
        {
            command.Parameters.Add(parameterName, SqlDbType.DateTime2).Value = value;
        }

        private static void AddNullableDateTime(SqlCommand command, string parameterName, DateTime? value)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.DateTime2);
            parameter.Value = value.HasValue ? (object)value.Value : DBNull.Value;
        }

        private static int GetInt(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.GetInt32(ordinal);
        }

        private static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (int?)null : reader.GetInt32(ordinal);
        }

        private static Guid GetGuid(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? Guid.Empty : reader.GetGuid(ordinal);
        }

        private static string GetString(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.GetString(ordinal);
        }

        private static string GetNullableString(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (decimal?)null : reader.GetDecimal(ordinal);
        }

        private static bool GetBool(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }

        private static DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.GetDateTime(ordinal);
        }

        private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? (DateTime?)null : reader.GetDateTime(ordinal);
        }
    }
}