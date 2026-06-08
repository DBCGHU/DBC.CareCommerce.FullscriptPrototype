using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Data.DataAccess;

namespace DBC.CareCommerce.Data.Repositories
{
    public sealed class SqlFullscriptTransactionRepository : IFullscriptTransactionRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public SqlFullscriptTransactionRepository(SqlConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            _connectionFactory = connectionFactory;
        }

        public FullscriptTransactionDto GetById(int fullscriptTransactionId)
        {
            const string sql = @"
SELECT
    FullscriptTransactionID,
    FullscriptTransactionGuid,
    CareItemID,
    CatalogItemID,
    PatientID,
    PatientCaseID,
    ProviderID,
    FullscriptPatientID,
    FullscriptPractitionerID,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptTreatmentPlanID,
    FullscriptOrderID,
    FullscriptOrderNumber,
    TreatmentPlanState,
    OrderStatus,
    InvitationUrl,
    CompletedAt,
    ItemTotal,
    MsrpTotal,
    PaymentTotal,
    LastSyncedDateTime,
    Status,
    ErrorMessage,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.FullscriptTransaction
WHERE FullscriptTransactionID = @FullscriptTransactionID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@FullscriptTransactionID", SqlDbType.Int).Value = fullscriptTransactionId;

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

        public IList<FullscriptTransactionDto> GetByCareItemId(int careItemId)
        {
            const string sql = @"
SELECT
    FullscriptTransactionID,
    FullscriptTransactionGuid,
    CareItemID,
    CatalogItemID,
    PatientID,
    PatientCaseID,
    ProviderID,
    FullscriptPatientID,
    FullscriptPractitionerID,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptTreatmentPlanID,
    FullscriptOrderID,
    FullscriptOrderNumber,
    TreatmentPlanState,
    OrderStatus,
    InvitationUrl,
    CompletedAt,
    ItemTotal,
    MsrpTotal,
    PaymentTotal,
    LastSyncedDateTime,
    Status,
    ErrorMessage,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.FullscriptTransaction
WHERE
    CareItemID = @CareItemID
    AND Active = 1
ORDER BY FullscriptTransactionID;";

            List<FullscriptTransactionDto> results = new List<FullscriptTransactionDto>();

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
                            results.Add(MapReader(reader));
                        }
                    }
                }
            }

            return results;
        }

        public IList<FullscriptTransactionDto> GetPendingTransactions()
        {
            const string sql = @"
SELECT
    FullscriptTransactionID,
    FullscriptTransactionGuid,
    CareItemID,
    CatalogItemID,
    PatientID,
    PatientCaseID,
    ProviderID,
    FullscriptPatientID,
    FullscriptPractitionerID,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptTreatmentPlanID,
    FullscriptOrderID,
    FullscriptOrderNumber,
    TreatmentPlanState,
    OrderStatus,
    InvitationUrl,
    CompletedAt,
    ItemTotal,
    MsrpTotal,
    PaymentTotal,
    LastSyncedDateTime,
    Status,
    ErrorMessage,
    CreatedDateTime,
    UpdatedDateTime,
    Active
FROM dbo.FullscriptTransaction
WHERE
    Status = N'ReadyToSend'
    AND Active = 1
ORDER BY FullscriptTransactionID;";

            List<FullscriptTransactionDto> results = new List<FullscriptTransactionDto>();

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(MapReader(reader));
                    }
                }
            }

            return results;
        }

        public int Insert(FullscriptTransactionDto transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            const string sql = @"
INSERT INTO dbo.FullscriptTransaction
(
    CareItemID,
    CatalogItemID,
    PatientID,
    PatientCaseID,
    ProviderID,
    FullscriptPatientID,
    FullscriptPractitionerID,
    FullscriptProductID,
    FullscriptVariantID,
    FullscriptTreatmentPlanID,
    FullscriptOrderID,
    FullscriptOrderNumber,
    TreatmentPlanState,
    OrderStatus,
    InvitationUrl,
    CompletedAt,
    ItemTotal,
    MsrpTotal,
    PaymentTotal,
    LastSyncedDateTime,
    Status,
    ErrorMessage,
    CreatedDateTime,
    UpdatedDateTime,
    Active
)
VALUES
(
    @CareItemID,
    @CatalogItemID,
    @PatientID,
    @PatientCaseID,
    @ProviderID,
    @FullscriptPatientID,
    @FullscriptPractitionerID,
    @FullscriptProductID,
    @FullscriptVariantID,
    @FullscriptTreatmentPlanID,
    @FullscriptOrderID,
    @FullscriptOrderNumber,
    @TreatmentPlanState,
    @OrderStatus,
    @InvitationUrl,
    @CompletedAt,
    @ItemTotal,
    @MsrpTotal,
    @PaymentTotal,
    @LastSyncedDateTime,
    @Status,
    @ErrorMessage,
    SYSUTCDATETIME(),
    @UpdatedDateTime,
    @Active
);

SELECT CONVERT(INT, SCOPE_IDENTITY());";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, transaction);

                    object result = command.ExecuteScalar();

                    int newId = Convert.ToInt32(result);
                    transaction.FullscriptTransactionId = newId;

                    return newId;
                }
            }
        }

        public void Update(FullscriptTransactionDto transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            if (!transaction.FullscriptTransactionId.HasValue)
            {
                throw new InvalidOperationException("FullscriptTransactionId is required for update.");
            }

            const string sql = @"
UPDATE dbo.FullscriptTransaction
SET
    CareItemID = @CareItemID,
    CatalogItemID = @CatalogItemID,
    PatientID = @PatientID,
    PatientCaseID = @PatientCaseID,
    ProviderID = @ProviderID,
    FullscriptPatientID = @FullscriptPatientID,
    FullscriptPractitionerID = @FullscriptPractitionerID,
    FullscriptProductID = @FullscriptProductID,
    FullscriptVariantID = @FullscriptVariantID,
    FullscriptTreatmentPlanID = @FullscriptTreatmentPlanID,
    FullscriptOrderID = @FullscriptOrderID,
    FullscriptOrderNumber = @FullscriptOrderNumber,
    TreatmentPlanState = @TreatmentPlanState,
    OrderStatus = @OrderStatus,
    InvitationUrl = @InvitationUrl,
    CompletedAt = @CompletedAt,
    ItemTotal = @ItemTotal,
    MsrpTotal = @MsrpTotal,
    PaymentTotal = @PaymentTotal,
    LastSyncedDateTime = @LastSyncedDateTime,
    Status = @Status,
    ErrorMessage = @ErrorMessage,
    UpdatedDateTime = SYSUTCDATETIME(),
    Active = @Active
WHERE FullscriptTransactionID = @FullscriptTransactionID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, transaction);
                    command.Parameters.Add("@FullscriptTransactionID", SqlDbType.Int).Value = transaction.FullscriptTransactionId.Value;

                    command.ExecuteNonQuery();
                }
            }
        }

        public void MarkSent(int fullscriptTransactionId, string externalReferenceId)
        {
            const string sql = @"
UPDATE dbo.FullscriptTransaction
SET
    Status = N'Sent',
    FullscriptTreatmentPlanID = @FullscriptTreatmentPlanID,
    UpdatedDateTime = SYSUTCDATETIME()
WHERE FullscriptTransactionID = @FullscriptTransactionID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@FullscriptTransactionID", SqlDbType.Int).Value = fullscriptTransactionId;
                    command.Parameters.Add("@FullscriptTreatmentPlanID", SqlDbType.NVarChar, 200).Value =
                        ToDbValue(externalReferenceId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void MarkFailed(int fullscriptTransactionId, string errorMessage)
        {
            const string sql = @"
UPDATE dbo.FullscriptTransaction
SET
    Status = N'Failed',
    ErrorMessage = @ErrorMessage,
    UpdatedDateTime = SYSUTCDATETIME()
WHERE FullscriptTransactionID = @FullscriptTransactionID;";

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@FullscriptTransactionID", SqlDbType.Int).Value = fullscriptTransactionId;
                    command.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar).Value = ToDbValue(errorMessage);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static void AddParameters(SqlCommand command, FullscriptTransactionDto transaction)
        {
            command.Parameters.Add("@CareItemID", SqlDbType.Int).Value = ToDbValue(transaction.CareItemId);
            command.Parameters.Add("@CatalogItemID", SqlDbType.Int).Value = ToDbValue(transaction.CatalogItemId);

            command.Parameters.Add("@PatientID", SqlDbType.Int).Value = transaction.PatientId;
            command.Parameters.Add("@PatientCaseID", SqlDbType.Int).Value = ToDbValue(transaction.PatientCaseId);
            command.Parameters.Add("@ProviderID", SqlDbType.Int).Value = ToDbValue(transaction.ProviderId);

            command.Parameters.Add("@FullscriptPatientID", SqlDbType.NVarChar, 200).Value =
                ToDbValue(transaction.FullscriptPatientId);
            command.Parameters.Add("@FullscriptPractitionerID", SqlDbType.NVarChar, 200).Value =
                ToDbValue(transaction.FullscriptPractitionerId);
            command.Parameters.Add("@FullscriptProductID", SqlDbType.NVarChar, 200).Value =
                ToDbValue(transaction.FullscriptProductId);
            command.Parameters.Add("@FullscriptVariantID", SqlDbType.NVarChar, 200).Value =
                ToDbValue(transaction.FullscriptVariantId);

            command.Parameters.Add("@FullscriptTreatmentPlanID", SqlDbType.NVarChar, 200).Value =
                ToDbValue(transaction.FullscriptTreatmentPlanId);
            command.Parameters.Add("@FullscriptOrderID", SqlDbType.NVarChar, 200).Value =
                ToDbValue(transaction.FullscriptOrderId);
            command.Parameters.Add("@FullscriptOrderNumber", SqlDbType.NVarChar, 200).Value =
                ToDbValue(transaction.FullscriptOrderNumber);

            command.Parameters.Add("@TreatmentPlanState", SqlDbType.NVarChar, 100).Value =
                ToDbValue(transaction.TreatmentPlanState);
            command.Parameters.Add("@OrderStatus", SqlDbType.NVarChar, 100).Value =
                ToDbValue(transaction.OrderStatus);

            command.Parameters.Add("@InvitationUrl", SqlDbType.NVarChar, 1000).Value =
                ToDbValue(transaction.InvitationUrl);

            command.Parameters.Add("@CompletedAt", SqlDbType.DateTime2).Value =
                ToDbValue(transaction.CompletedAt);

            AddNullableDecimal(command, "@ItemTotal", transaction.ItemTotal);
            AddNullableDecimal(command, "@MsrpTotal", transaction.MsrpTotal);
            AddNullableDecimal(command, "@PaymentTotal", transaction.PaymentTotal);

            command.Parameters.Add("@LastSyncedDateTime", SqlDbType.DateTime2).Value =
                ToDbValue(transaction.LastSyncedDateTime);

            command.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value =
                ToDbValue(transaction.Status ?? "Pending");

            command.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar).Value =
                ToDbValue(transaction.ErrorMessage);

            command.Parameters.Add("@UpdatedDateTime", SqlDbType.DateTime2).Value =
                ToDbValue(transaction.UpdatedDateTime);

            command.Parameters.Add("@Active", SqlDbType.Bit).Value = transaction.Active;
        }

        private static void AddNullableDecimal(SqlCommand command, string parameterName, decimal? value)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.Decimal);
            parameter.Precision = 18;
            parameter.Scale = 2;
            parameter.Value = ToDbValue(value);
        }

        private static FullscriptTransactionDto MapReader(SqlDataReader reader)
        {
            return new FullscriptTransactionDto
            {
                FullscriptTransactionId = GetNullableInt(reader, "FullscriptTransactionID"),
                FullscriptTransactionGuid = GetGuid(reader, "FullscriptTransactionGuid"),
                CareItemId = GetNullableInt(reader, "CareItemID"),
                CatalogItemId = GetNullableInt(reader, "CatalogItemID"),
                PatientId = GetInt(reader, "PatientID"),
                PatientCaseId = GetNullableInt(reader, "PatientCaseID"),
                ProviderId = GetNullableInt(reader, "ProviderID"),
                FullscriptPatientId = GetNullableString(reader, "FullscriptPatientID"),
                FullscriptPractitionerId = GetNullableString(reader, "FullscriptPractitionerID"),
                FullscriptProductId = GetNullableString(reader, "FullscriptProductID"),
                FullscriptVariantId = GetNullableString(reader, "FullscriptVariantID"),
                FullscriptTreatmentPlanId = GetNullableString(reader, "FullscriptTreatmentPlanID"),
                FullscriptOrderId = GetNullableString(reader, "FullscriptOrderID"),
                FullscriptOrderNumber = GetNullableString(reader, "FullscriptOrderNumber"),
                TreatmentPlanState = GetNullableString(reader, "TreatmentPlanState"),
                OrderStatus = GetNullableString(reader, "OrderStatus"),
                InvitationUrl = GetNullableString(reader, "InvitationUrl"),
                CompletedAt = GetNullableDateTime(reader, "CompletedAt"),
                ItemTotal = GetNullableDecimal(reader, "ItemTotal"),
                MsrpTotal = GetNullableDecimal(reader, "MsrpTotal"),
                PaymentTotal = GetNullableDecimal(reader, "PaymentTotal"),
                LastSyncedDateTime = GetNullableDateTime(reader, "LastSyncedDateTime"),
                Status = GetString(reader, "Status"),
                ErrorMessage = GetNullableString(reader, "ErrorMessage"),
                CreatedDateTime = GetDateTime(reader, "CreatedDateTime"),
                UpdatedDateTime = GetNullableDateTime(reader, "UpdatedDateTime"),
                Active = GetBool(reader, "Active")
            };
        }

        private static object ToDbValue(object value)
        {
            return value ?? DBNull.Value;
        }

        private static int GetInt(SqlDataReader reader, string columnName)
        {
            return Convert.ToInt32(reader[columnName]);
        }

        private static int? GetNullableInt(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(value);
        }

        private static Guid GetGuid(SqlDataReader reader, string columnName)
        {
            return (Guid)reader[columnName];
        }

        private static string GetString(SqlDataReader reader, string columnName)
        {
            return Convert.ToString(reader[columnName]);
        }

        private static string GetNullableString(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == DBNull.Value)
            {
                return null;
            }

            return Convert.ToString(value);
        }

        private static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDecimal(value);
        }

        private static DateTime GetDateTime(SqlDataReader reader, string columnName)
        {
            return Convert.ToDateTime(reader[columnName]);
        }

        private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(value);
        }

        private static bool GetBool(SqlDataReader reader, string columnName)
        {
            return Convert.ToBoolean(reader[columnName]);
        }
    }
}