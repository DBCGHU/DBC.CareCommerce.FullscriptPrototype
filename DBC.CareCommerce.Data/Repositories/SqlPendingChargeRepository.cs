using System;
using System.Collections.Generic;
using System.Data;
using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.DataAccess;
using Microsoft.Data.SqlClient;

namespace DBC.CareCommerce.Data.Repositories
{
    public sealed class SqlPendingChargeRepository : IPendingChargeRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public SqlPendingChargeRepository(SqlConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            _connectionFactory = connectionFactory;
        }

        public PendingChargeDto GetById(int pendingChargeId)
        {
            const string sql = @"
SELECT
    PendingChargeID,
    PendingChargeGuid,
    CareItemID,
    PatientID,
    PatientCaseID,
    ProviderID,
    CatalogItemID,
    FeeID,
    ProductID,
    Description,
    Quantity,
    UnitAmount,
    TotalAmount,
    BillingAction,
    InventoryAction,
    FulfillmentSource,
    Status,
    ApprovedByUserID,
    ApprovedDateTime,
    RejectedByUserID,
    RejectedDateTime,
    RejectionReason,
    PostedDateTime,
    PostingID,
    ErrorMessage,
    CreatedByUserID,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.PendingCharge
WHERE PendingChargeID = @PendingChargeID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@PendingChargeID", SqlDbType.Int).Value = pendingChargeId;

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

        public IList<PendingChargeDto> GetPendingForPatientCase(int patientId, int patientCaseId)
        {
            List<PendingChargeDto> items = new List<PendingChargeDto>();

            const string sql = @"
SELECT
    PendingChargeID,
    PendingChargeGuid,
    CareItemID,
    PatientID,
    PatientCaseID,
    ProviderID,
    CatalogItemID,
    FeeID,
    ProductID,
    Description,
    Quantity,
    UnitAmount,
    TotalAmount,
    BillingAction,
    InventoryAction,
    FulfillmentSource,
    Status,
    ApprovedByUserID,
    ApprovedDateTime,
    RejectedByUserID,
    RejectedDateTime,
    RejectionReason,
    PostedDateTime,
    PostingID,
    ErrorMessage,
    CreatedByUserID,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.PendingCharge
WHERE
    Active = 1
    AND PatientID = @PatientID
    AND PatientCaseID = @PatientCaseID
    AND Status = @Status
ORDER BY PendingChargeID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@PatientID", SqlDbType.Int).Value = patientId;
                    command.Parameters.Add("@PatientCaseID", SqlDbType.Int).Value = patientCaseId;
                    AddNVarChar(command, "@Status", PendingChargeStatus.Pending.ToString(), 50);

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

        public IList<PendingChargeDto> GetByCareItemId(int careItemId)
        {
            List<PendingChargeDto> items = new List<PendingChargeDto>();

            const string sql = @"
SELECT
    PendingChargeID,
    PendingChargeGuid,
    CareItemID,
    PatientID,
    PatientCaseID,
    ProviderID,
    CatalogItemID,
    FeeID,
    ProductID,
    Description,
    Quantity,
    UnitAmount,
    TotalAmount,
    BillingAction,
    InventoryAction,
    FulfillmentSource,
    Status,
    ApprovedByUserID,
    ApprovedDateTime,
    RejectedByUserID,
    RejectedDateTime,
    RejectionReason,
    PostedDateTime,
    PostingID,
    ErrorMessage,
    CreatedByUserID,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.PendingCharge
WHERE
    Active = 1
    AND CareItemID = @CareItemID
ORDER BY PendingChargeID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@CareItemID", SqlDbType.Int).Value = careItemId;

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

        public int Insert(PendingChargeDto charge)
        {
            if (charge == null)
            {
                throw new ArgumentNullException("charge");
            }

            if (charge.PatientId <= 0)
            {
                throw new InvalidOperationException("PatientId is required.");
            }

            if (charge.PatientCaseId <= 0)
            {
                throw new InvalidOperationException("PatientCaseId is required.");
            }

            if (charge.PendingChargeGuid == Guid.Empty)
            {
                charge.PendingChargeGuid = Guid.NewGuid();
            }

            if (string.IsNullOrWhiteSpace(charge.Description))
            {
                throw new InvalidOperationException("Description is required.");
            }

            if (charge.Quantity <= 0)
            {
                charge.Quantity = 1m;
            }

            if (charge.CreatedDateTime == DateTime.MinValue)
            {
                charge.CreatedDateTime = DateTime.UtcNow;
            }

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
INSERT INTO dbo.PendingCharge
(
    PendingChargeGuid,
    CareItemID,
    PatientID,
    PatientCaseID,
    ProviderID,
    CatalogItemID,
    FeeID,
    ProductID,
    Description,
    Quantity,
    UnitAmount,
    TotalAmount,
    BillingAction,
    InventoryAction,
    FulfillmentSource,
    Status,
    ApprovedByUserID,
    ApprovedDateTime,
    RejectedByUserID,
    RejectedDateTime,
    RejectionReason,
    PostedDateTime,
    PostingID,
    ErrorMessage,
    CreatedByUserID,
    CreatedDateTime,
    UpdatedDateTime,
    Active
)
VALUES
(
    @PendingChargeGuid,
    @CareItemID,
    @PatientID,
    @PatientCaseID,
    @ProviderID,
    @CatalogItemID,
    @FeeID,
    @ProductID,
    @Description,
    @Quantity,
    @UnitAmount,
    @TotalAmount,
    @BillingAction,
    @InventoryAction,
    @FulfillmentSource,
    @Status,
    @ApprovedByUserID,
    @ApprovedDateTime,
    @RejectedByUserID,
    @RejectedDateTime,
    @RejectionReason,
    @PostedDateTime,
    @PostingID,
    @ErrorMessage,
    @CreatedByUserID,
    @CreatedDateTime,
    @UpdatedDateTime,
    @Active
);

SELECT CONVERT(int, SCOPE_IDENTITY());";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, charge);

                    int newId = Convert.ToInt32(command.ExecuteScalar());
                    charge.PendingChargeId = newId;

                    return newId;
                }
            }
        }

        public void Update(PendingChargeDto charge)
        {
            if (charge == null)
            {
                throw new ArgumentNullException("charge");
            }

            if (!charge.PendingChargeId.HasValue)
            {
                throw new InvalidOperationException("PendingChargeId is required for update.");
            }

            charge.UpdatedDateTime = DateTime.UtcNow;

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
UPDATE dbo.PendingCharge
SET
    PendingChargeGuid = @PendingChargeGuid,
    CareItemID = @CareItemID,
    PatientID = @PatientID,
    PatientCaseID = @PatientCaseID,
    ProviderID = @ProviderID,
    CatalogItemID = @CatalogItemID,
    FeeID = @FeeID,
    ProductID = @ProductID,
    Description = @Description,
    Quantity = @Quantity,
    UnitAmount = @UnitAmount,
    TotalAmount = @TotalAmount,
    BillingAction = @BillingAction,
    InventoryAction = @InventoryAction,
    FulfillmentSource = @FulfillmentSource,
    Status = @Status,
    ApprovedByUserID = @ApprovedByUserID,
    ApprovedDateTime = @ApprovedDateTime,
    RejectedByUserID = @RejectedByUserID,
    RejectedDateTime = @RejectedDateTime,
    RejectionReason = @RejectionReason,
    PostedDateTime = @PostedDateTime,
    PostingID = @PostingID,
    ErrorMessage = @ErrorMessage,
    CreatedByUserID = @CreatedByUserID,
    CreatedDateTime = @CreatedDateTime,
    UpdatedDateTime = @UpdatedDateTime,
    Active = @Active
WHERE PendingChargeID = @PendingChargeID;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, charge);
                    command.Parameters.Add("@PendingChargeID", SqlDbType.Int).Value = charge.PendingChargeId.Value;

                    int affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("Pending charge was not found.");
                    }
                }
            }
        }

        public void MarkApproved(int pendingChargeId, int userId)
        {
            const string sql = @"
UPDATE dbo.PendingCharge
SET
    Status = @Status,
    ApprovedByUserID = @ApprovedByUserID,
    ApprovedDateTime = SYSUTCDATETIME(),
    UpdatedDateTime = SYSUTCDATETIME()
WHERE PendingChargeID = @PendingChargeID;";

            ExecuteStatusUpdate(sql, pendingChargeId, command =>
            {
                AddNVarChar(command, "@Status", PendingChargeStatus.Approved.ToString(), 50);
                command.Parameters.Add("@ApprovedByUserID", SqlDbType.Int).Value = userId;
            });
        }

        public void MarkRejected(int pendingChargeId, int userId, string reason)
        {
            const string sql = @"
UPDATE dbo.PendingCharge
SET
    Status = @Status,
    RejectedByUserID = @RejectedByUserID,
    RejectedDateTime = SYSUTCDATETIME(),
    RejectionReason = @RejectionReason,
    UpdatedDateTime = SYSUTCDATETIME()
WHERE PendingChargeID = @PendingChargeID;";

            ExecuteStatusUpdate(sql, pendingChargeId, command =>
            {
                AddNVarChar(command, "@Status", PendingChargeStatus.Rejected.ToString(), 50);
                command.Parameters.Add("@RejectedByUserID", SqlDbType.Int).Value = userId;
                AddNullableNVarChar(command, "@RejectionReason", reason, 500);
            });
        }

        public void MarkPosted(int pendingChargeId, int postingId)
        {
            const string sql = @"
UPDATE dbo.PendingCharge
SET
    Status = @Status,
    PostingID = @PostingID,
    PostedDateTime = SYSUTCDATETIME(),
    UpdatedDateTime = SYSUTCDATETIME()
WHERE PendingChargeID = @PendingChargeID;";

            ExecuteStatusUpdate(sql, pendingChargeId, command =>
            {
                AddNVarChar(command, "@Status", PendingChargeStatus.Posted.ToString(), 50);
                command.Parameters.Add("@PostingID", SqlDbType.Int).Value = postingId;
            });
        }

        private void ExecuteStatusUpdate(string sql, int pendingChargeId, Action<SqlCommand> addParameters)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@PendingChargeID", SqlDbType.Int).Value = pendingChargeId;
                    addParameters(command);

                    int affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("Pending charge was not found.");
                    }
                }
            }
        }

        private static PendingChargeDto MapReader(SqlDataReader reader)
        {
            return new PendingChargeDto
            {
                PendingChargeId = GetNullableInt(reader, "PendingChargeID"),
                PendingChargeGuid = GetGuid(reader, "PendingChargeGuid"),
                CareItemId = GetNullableInt(reader, "CareItemID"),
                PatientId = GetInt(reader, "PatientID"),
                PatientCaseId = GetInt(reader, "PatientCaseID"),
                ProviderId = GetNullableInt(reader, "ProviderID"),
                CatalogItemId = GetNullableInt(reader, "CatalogItemID"),
                FeeId = GetNullableInt(reader, "FeeID"),
                ProductId = GetNullableInt(reader, "ProductID"),
                Description = GetString(reader, "Description"),
                Quantity = GetDecimal(reader, "Quantity"),
                UnitAmount = GetNullableDecimal(reader, "UnitAmount"),
                TotalAmount = GetNullableDecimal(reader, "TotalAmount"),
                BillingAction = ParseBillingAction(GetString(reader, "BillingAction")),
                InventoryAction = ParseInventoryAction(GetString(reader, "InventoryAction")),
                FulfillmentSource = ParseFulfillmentSource(GetString(reader, "FulfillmentSource")),
                Status = ParsePendingChargeStatus(GetString(reader, "Status")),
                ApprovedByUserId = GetNullableInt(reader, "ApprovedByUserID"),
                ApprovedDateTime = GetNullableDateTime(reader, "ApprovedDateTime"),
                RejectedByUserId = GetNullableInt(reader, "RejectedByUserID"),
                RejectedDateTime = GetNullableDateTime(reader, "RejectedDateTime"),
                RejectionReason = GetNullableString(reader, "RejectionReason"),
                PostedDateTime = GetNullableDateTime(reader, "PostedDateTime"),
                PostingId = GetNullableInt(reader, "PostingID"),
                ErrorMessage = GetNullableString(reader, "ErrorMessage"),
                CreatedByUserId = GetNullableInt(reader, "CreatedByUserID"),
                CreatedDateTime = GetDateTime(reader, "CreatedDateTime"),
                UpdatedDateTime = GetNullableDateTime(reader, "UpdatedDateTime"),
                Active = GetBool(reader, "Active")
            };
        }

        private static void AddParameters(SqlCommand command, PendingChargeDto charge)
        {
            command.Parameters.Add("@PendingChargeGuid", SqlDbType.UniqueIdentifier).Value = charge.PendingChargeGuid;

            AddNullableInt(command, "@CareItemID", charge.CareItemId);
            command.Parameters.Add("@PatientID", SqlDbType.Int).Value = charge.PatientId;
            command.Parameters.Add("@PatientCaseID", SqlDbType.Int).Value = charge.PatientCaseId;
            AddNullableInt(command, "@ProviderID", charge.ProviderId);
            AddNullableInt(command, "@CatalogItemID", charge.CatalogItemId);
            AddNullableInt(command, "@FeeID", charge.FeeId);
            AddNullableInt(command, "@ProductID", charge.ProductId);

            AddNVarChar(command, "@Description", charge.Description, 255);
            AddDecimal(command, "@Quantity", charge.Quantity, 18, 4);
            AddNullableDecimal(command, "@UnitAmount", charge.UnitAmount, 18, 2);
            AddNullableDecimal(command, "@TotalAmount", charge.TotalAmount, 18, 2);

            AddNVarChar(command, "@BillingAction", charge.BillingAction.ToString(), 50);
            AddNVarChar(command, "@InventoryAction", charge.InventoryAction.ToString(), 50);
            AddNVarChar(command, "@FulfillmentSource", charge.FulfillmentSource.ToString(), 50);
            AddNVarChar(command, "@Status", charge.Status.ToString(), 50);

            AddNullableInt(command, "@ApprovedByUserID", charge.ApprovedByUserId);
            AddNullableDateTime(command, "@ApprovedDateTime", charge.ApprovedDateTime);
            AddNullableInt(command, "@RejectedByUserID", charge.RejectedByUserId);
            AddNullableDateTime(command, "@RejectedDateTime", charge.RejectedDateTime);
            AddNullableNVarChar(command, "@RejectionReason", charge.RejectionReason, 500);
            AddNullableDateTime(command, "@PostedDateTime", charge.PostedDateTime);
            AddNullableInt(command, "@PostingID", charge.PostingId);
            AddNullableNVarChar(command, "@ErrorMessage", charge.ErrorMessage, -1);
            AddNullableInt(command, "@CreatedByUserID", charge.CreatedByUserId);
            AddDateTime(command, "@CreatedDateTime", charge.CreatedDateTime);
            AddNullableDateTime(command, "@UpdatedDateTime", charge.UpdatedDateTime);
            AddBool(command, "@Active", charge.Active);
        }

        private static PendingChargeStatus ParsePendingChargeStatus(string value)
        {
            PendingChargeStatus parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : PendingChargeStatus.Pending;
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

        private static FulfillmentSource ParseFulfillmentSource(string value)
        {
            FulfillmentSource parsed;
            return Enum.TryParse(value, true, out parsed) ? parsed : FulfillmentSource.None;
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

        private static void AddDecimal(SqlCommand command, string parameterName, decimal value, byte precision, byte scale)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.Decimal);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value;
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

        private static decimal GetDecimal(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.GetDecimal(ordinal);
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