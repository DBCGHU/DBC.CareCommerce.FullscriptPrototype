using System;
using System.Collections.Generic;
using System.Data;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.DataAccess;
using Microsoft.Data.SqlClient;

namespace DBC.CareCommerce.Data.Repositories
{
    public sealed class SqlFullscriptConnectionRepository : IFullscriptConnectionRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public SqlFullscriptConnectionRepository(SqlConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            _connectionFactory = connectionFactory;
        }

        public FullscriptConnectionDto GetById(int fullscriptConnectionId)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT
    FullscriptConnectionId,
    FullscriptConnectionGuid,
    Environment,
    ClinicId,
    ClinicName,
    PractitionerId,
    PractitionerType,
    ClientId,
    AccessTokenEncrypted,
    RefreshTokenEncrypted,
    TokenType,
    Scope,
    TokenReceivedDateTime,
    TokenExpiresAtDateTime,
    LastRefreshDateTime,
    DispensaryUrl,
    IntegrationId,
    IntegrationActivatedAt,
    Status,
    ErrorMessage,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.FullscriptConnection
WHERE FullscriptConnectionId = @FullscriptConnectionId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@FullscriptConnectionId", SqlDbType.Int).Value = fullscriptConnectionId;

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

        public FullscriptConnectionDto GetActiveByEnvironmentAndClinic(string environment, string clinicId)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT TOP 1
    FullscriptConnectionId,
    FullscriptConnectionGuid,
    Environment,
    ClinicId,
    ClinicName,
    PractitionerId,
    PractitionerType,
    ClientId,
    AccessTokenEncrypted,
    RefreshTokenEncrypted,
    TokenType,
    Scope,
    TokenReceivedDateTime,
    TokenExpiresAtDateTime,
    LastRefreshDateTime,
    DispensaryUrl,
    IntegrationId,
    IntegrationActivatedAt,
    Status,
    ErrorMessage,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.FullscriptConnection
WHERE
    Active = 1
    AND Environment = @Environment
    AND ISNULL(ClinicId, '') = ISNULL(@ClinicId, '')
ORDER BY FullscriptConnectionId DESC;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddNullableNVarChar(command, "@Environment", environment, 50);
                    AddNullableNVarChar(command, "@ClinicId", clinicId, 100);

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

        public IList<FullscriptConnectionDto> GetActiveByEnvironment(string environment)
        {
            List<FullscriptConnectionDto> items = new List<FullscriptConnectionDto>();

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT
    FullscriptConnectionId,
    FullscriptConnectionGuid,
    Environment,
    ClinicId,
    ClinicName,
    PractitionerId,
    PractitionerType,
    ClientId,
    AccessTokenEncrypted,
    RefreshTokenEncrypted,
    TokenType,
    Scope,
    TokenReceivedDateTime,
    TokenExpiresAtDateTime,
    LastRefreshDateTime,
    DispensaryUrl,
    IntegrationId,
    IntegrationActivatedAt,
    Status,
    ErrorMessage,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.FullscriptConnection
WHERE
    Active = 1
    AND Environment = @Environment
ORDER BY FullscriptConnectionId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddNullableNVarChar(command, "@Environment", environment, 50);

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

        public int Insert(FullscriptConnectionDto connectionDto)
        {
            if (connectionDto == null)
            {
                throw new ArgumentNullException("connectionDto");
            }

            if (string.IsNullOrWhiteSpace(connectionDto.Environment))
            {
                throw new InvalidOperationException("Environment is required.");
            }

            if (connectionDto.FullscriptConnectionGuid == Guid.Empty)
            {
                connectionDto.FullscriptConnectionGuid = Guid.NewGuid();
            }

            if (connectionDto.CreatedDateTime == DateTime.MinValue)
            {
                connectionDto.CreatedDateTime = DateTime.UtcNow;
            }

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
INSERT INTO dbo.FullscriptConnection
(
    FullscriptConnectionGuid,
    Environment,
    ClinicId,
    ClinicName,
    PractitionerId,
    PractitionerType,
    ClientId,
    AccessTokenEncrypted,
    RefreshTokenEncrypted,
    TokenType,
    Scope,
    TokenReceivedDateTime,
    TokenExpiresAtDateTime,
    LastRefreshDateTime,
    DispensaryUrl,
    IntegrationId,
    IntegrationActivatedAt,
    Status,
    ErrorMessage,
    Active,
    CreatedDateTime,
    UpdatedDateTime
)
VALUES
(
    @FullscriptConnectionGuid,
    @Environment,
    @ClinicId,
    @ClinicName,
    @PractitionerId,
    @PractitionerType,
    @ClientId,
    @AccessTokenEncrypted,
    @RefreshTokenEncrypted,
    @TokenType,
    @Scope,
    @TokenReceivedDateTime,
    @TokenExpiresAtDateTime,
    @LastRefreshDateTime,
    @DispensaryUrl,
    @IntegrationId,
    @IntegrationActivatedAt,
    @Status,
    @ErrorMessage,
    @Active,
    @CreatedDateTime,
    @UpdatedDateTime
);

SELECT CONVERT(int, SCOPE_IDENTITY());";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, connectionDto);

                    int newId = Convert.ToInt32(command.ExecuteScalar());
                    connectionDto.FullscriptConnectionId = newId;

                    return newId;
                }
            }
        }

        public void Update(FullscriptConnectionDto connectionDto)
        {
            if (connectionDto == null)
            {
                throw new ArgumentNullException("connectionDto");
            }

            if (!connectionDto.FullscriptConnectionId.HasValue)
            {
                throw new InvalidOperationException("FullscriptConnectionId is required for update.");
            }

            connectionDto.UpdatedDateTime = DateTime.UtcNow;

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
UPDATE dbo.FullscriptConnection
SET
    FullscriptConnectionGuid = @FullscriptConnectionGuid,
    Environment = @Environment,
    ClinicId = @ClinicId,
    ClinicName = @ClinicName,
    PractitionerId = @PractitionerId,
    PractitionerType = @PractitionerType,
    ClientId = @ClientId,
    AccessTokenEncrypted = @AccessTokenEncrypted,
    RefreshTokenEncrypted = @RefreshTokenEncrypted,
    TokenType = @TokenType,
    Scope = @Scope,
    TokenReceivedDateTime = @TokenReceivedDateTime,
    TokenExpiresAtDateTime = @TokenExpiresAtDateTime,
    LastRefreshDateTime = @LastRefreshDateTime,
    DispensaryUrl = @DispensaryUrl,
    IntegrationId = @IntegrationId,
    IntegrationActivatedAt = @IntegrationActivatedAt,
    Status = @Status,
    ErrorMessage = @ErrorMessage,
    Active = @Active,
    CreatedDateTime = @CreatedDateTime,
    UpdatedDateTime = @UpdatedDateTime
WHERE FullscriptConnectionId = @FullscriptConnectionId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, connectionDto);
                    command.Parameters.Add("@FullscriptConnectionId", SqlDbType.Int).Value =
                        connectionDto.FullscriptConnectionId.Value;

                    int affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("Fullscript connection was not found.");
                    }
                }
            }
        }

        public void MarkInactive(int fullscriptConnectionId)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
UPDATE dbo.FullscriptConnection
SET
    Active = 0,
    Status = @Status,
    UpdatedDateTime = SYSUTCDATETIME()
WHERE FullscriptConnectionId = @FullscriptConnectionId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddNullableNVarChar(command, "@Status", "Inactive", 50);
                    command.Parameters.Add("@FullscriptConnectionId", SqlDbType.Int).Value = fullscriptConnectionId;

                    int affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("Fullscript connection was not found.");
                    }
                }
            }
        }

        public void SaveTokenState(FullscriptConnectionDto connectionDto)
        {
            if (connectionDto == null)
            {
                throw new ArgumentNullException("connectionDto");
            }

            if (connectionDto.FullscriptConnectionId.HasValue &&
                GetById(connectionDto.FullscriptConnectionId.Value) != null)
            {
                Update(connectionDto);
                return;
            }

            Insert(connectionDto);
        }

        private static FullscriptConnectionDto MapReader(SqlDataReader reader)
        {
            return new FullscriptConnectionDto
            {
                FullscriptConnectionId = GetNullableInt(reader, "FullscriptConnectionId"),
                FullscriptConnectionGuid = GetGuid(reader, "FullscriptConnectionGuid"),
                Environment = GetNullableString(reader, "Environment"),
                ClinicId = GetNullableString(reader, "ClinicId"),
                ClinicName = GetNullableString(reader, "ClinicName"),
                PractitionerId = GetNullableString(reader, "PractitionerId"),
                PractitionerType = GetNullableString(reader, "PractitionerType"),
                ClientId = GetNullableString(reader, "ClientId"),
                AccessTokenEncrypted = GetNullableString(reader, "AccessTokenEncrypted"),
                RefreshTokenEncrypted = GetNullableString(reader, "RefreshTokenEncrypted"),
                TokenType = GetNullableString(reader, "TokenType"),
                Scope = GetNullableString(reader, "Scope"),
                TokenReceivedDateTime = GetNullableDateTime(reader, "TokenReceivedDateTime"),
                TokenExpiresAtDateTime = GetNullableDateTime(reader, "TokenExpiresAtDateTime"),
                LastRefreshDateTime = GetNullableDateTime(reader, "LastRefreshDateTime"),
                DispensaryUrl = GetNullableString(reader, "DispensaryUrl"),
                IntegrationId = GetNullableString(reader, "IntegrationId"),
                IntegrationActivatedAt = GetNullableDateTime(reader, "IntegrationActivatedAt"),
                Status = GetNullableString(reader, "Status"),
                ErrorMessage = GetNullableString(reader, "ErrorMessage"),
                Active = GetBoolean(reader, "Active"),
                CreatedDateTime = GetDateTime(reader, "CreatedDateTime"),
                UpdatedDateTime = GetNullableDateTime(reader, "UpdatedDateTime")
            };
        }

        private static void AddParameters(SqlCommand command, FullscriptConnectionDto dto)
        {
            AddGuid(command, "@FullscriptConnectionGuid", dto.FullscriptConnectionGuid);
            AddNullableNVarChar(command, "@Environment", dto.Environment, 50);
            AddNullableNVarChar(command, "@ClinicId", dto.ClinicId, 100);
            AddNullableNVarChar(command, "@ClinicName", dto.ClinicName, 300);
            AddNullableNVarChar(command, "@PractitionerId", dto.PractitionerId, 100);
            AddNullableNVarChar(command, "@PractitionerType", dto.PractitionerType, 100);
            AddNullableNVarChar(command, "@ClientId", dto.ClientId, 200);
            AddNullableNVarChar(command, "@AccessTokenEncrypted", dto.AccessTokenEncrypted, -1);
            AddNullableNVarChar(command, "@RefreshTokenEncrypted", dto.RefreshTokenEncrypted, -1);
            AddNullableNVarChar(command, "@TokenType", dto.TokenType, 100);
            AddNullableNVarChar(command, "@Scope", dto.Scope, 1000);
            AddNullableDateTime(command, "@TokenReceivedDateTime", dto.TokenReceivedDateTime);
            AddNullableDateTime(command, "@TokenExpiresAtDateTime", dto.TokenExpiresAtDateTime);
            AddNullableDateTime(command, "@LastRefreshDateTime", dto.LastRefreshDateTime);
            AddNullableNVarChar(command, "@DispensaryUrl", dto.DispensaryUrl, 1000);
            AddNullableNVarChar(command, "@IntegrationId", dto.IntegrationId, 100);
            AddNullableDateTime(command, "@IntegrationActivatedAt", dto.IntegrationActivatedAt);
            AddNullableNVarChar(command, "@Status", dto.Status, 50);
            AddNullableNVarChar(command, "@ErrorMessage", dto.ErrorMessage, 2000);
            command.Parameters.Add("@Active", SqlDbType.Bit).Value = dto.Active;
            AddDateTime(command, "@CreatedDateTime", dto.CreatedDateTime);
            AddNullableDateTime(command, "@UpdatedDateTime", dto.UpdatedDateTime);
        }

        private static void AddGuid(SqlCommand command, string parameterName, Guid value)
        {
            command.Parameters.Add(parameterName, SqlDbType.UniqueIdentifier).Value = value;
        }

        private static void AddNullableNVarChar(SqlCommand command, string parameterName, string value, int size)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.NVarChar, size);
            parameter.Value = string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
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

        private static string GetNullableString(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
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

        private static bool GetBoolean(SqlDataReader reader, string columnName)
        {
            int ordinal = reader.GetOrdinal(columnName);
            return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
        }
    }
}