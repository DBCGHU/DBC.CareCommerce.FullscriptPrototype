using System;
using System.Collections.Generic;
using System.Data;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.DataAccess;
using Microsoft.Data.SqlClient;

namespace DBC.CareCommerce.Data.Repositories
{
    public sealed class SqlFullscriptPatientMapRepository : IFullscriptPatientMapRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public SqlFullscriptPatientMapRepository(SqlConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            _connectionFactory = connectionFactory;
        }

        public FullscriptPatientMapDto GetById(int fullscriptPatientMapId)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT
    FullscriptPatientMapId,
    FullscriptPatientMapGuid,
    PatientId,
    FullscriptPatientId,
    FullscriptMetadataId,
    FullscriptEmail,
    FullscriptFirstName,
    FullscriptLastName,
    Environment,
    ClinicId,
    LastSyncedDateTime,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.FullscriptPatientMap
WHERE FullscriptPatientMapId = @FullscriptPatientMapId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@FullscriptPatientMapId", SqlDbType.Int).Value = fullscriptPatientMapId;

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

        public FullscriptPatientMapDto GetByPatientId(int patientId, string environment, string clinicId)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT TOP 1
    FullscriptPatientMapId,
    FullscriptPatientMapGuid,
    PatientId,
    FullscriptPatientId,
    FullscriptMetadataId,
    FullscriptEmail,
    FullscriptFirstName,
    FullscriptLastName,
    Environment,
    ClinicId,
    LastSyncedDateTime,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.FullscriptPatientMap
WHERE
    Active = 1
    AND PatientId = @PatientId
    AND Environment = @Environment
    AND ISNULL(ClinicId, '') = ISNULL(@ClinicId, '')
ORDER BY FullscriptPatientMapId DESC;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;
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

        public FullscriptPatientMapDto GetByFullscriptPatientId(string fullscriptPatientId, string environment, string clinicId)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT TOP 1
    FullscriptPatientMapId,
    FullscriptPatientMapGuid,
    PatientId,
    FullscriptPatientId,
    FullscriptMetadataId,
    FullscriptEmail,
    FullscriptFirstName,
    FullscriptLastName,
    Environment,
    ClinicId,
    LastSyncedDateTime,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.FullscriptPatientMap
WHERE
    Active = 1
    AND FullscriptPatientId = @FullscriptPatientId
    AND Environment = @Environment
    AND ISNULL(ClinicId, '') = ISNULL(@ClinicId, '')
ORDER BY FullscriptPatientMapId DESC;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddNullableNVarChar(command, "@FullscriptPatientId", fullscriptPatientId, 100);
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

        public FullscriptPatientMapDto GetByMetadataId(string metadataId, string environment, string clinicId)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT TOP 1
    FullscriptPatientMapId,
    FullscriptPatientMapGuid,
    PatientId,
    FullscriptPatientId,
    FullscriptMetadataId,
    FullscriptEmail,
    FullscriptFirstName,
    FullscriptLastName,
    Environment,
    ClinicId,
    LastSyncedDateTime,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.FullscriptPatientMap
WHERE
    Active = 1
    AND FullscriptMetadataId = @FullscriptMetadataId
    AND Environment = @Environment
    AND ISNULL(ClinicId, '') = ISNULL(@ClinicId, '')
ORDER BY FullscriptPatientMapId DESC;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddNullableNVarChar(command, "@FullscriptMetadataId", metadataId, 100);
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

        public IList<FullscriptPatientMapDto> GetAllForPatient(int patientId)
        {
            List<FullscriptPatientMapDto> items = new List<FullscriptPatientMapDto>();

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT
    FullscriptPatientMapId,
    FullscriptPatientMapGuid,
    PatientId,
    FullscriptPatientId,
    FullscriptMetadataId,
    FullscriptEmail,
    FullscriptFirstName,
    FullscriptLastName,
    Environment,
    ClinicId,
    LastSyncedDateTime,
    Active,
    CreatedDateTime,
    UpdatedDateTime
FROM dbo.FullscriptPatientMap
WHERE PatientId = @PatientId
ORDER BY FullscriptPatientMapId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;

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

        public int Insert(FullscriptPatientMapDto map)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            if (map.PatientId <= 0)
            {
                throw new InvalidOperationException("PatientId is required.");
            }

            if (string.IsNullOrWhiteSpace(map.Environment))
            {
                throw new InvalidOperationException("Environment is required.");
            }

            if (map.FullscriptPatientMapGuid == Guid.Empty)
            {
                map.FullscriptPatientMapGuid = Guid.NewGuid();
            }

            if (map.CreatedDateTime == DateTime.MinValue)
            {
                map.CreatedDateTime = DateTime.UtcNow;
            }

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
INSERT INTO dbo.FullscriptPatientMap
(
    FullscriptPatientMapGuid,
    PatientId,
    FullscriptPatientId,
    FullscriptMetadataId,
    FullscriptEmail,
    FullscriptFirstName,
    FullscriptLastName,
    Environment,
    ClinicId,
    LastSyncedDateTime,
    Active,
    CreatedDateTime,
    UpdatedDateTime
)
VALUES
(
    @FullscriptPatientMapGuid,
    @PatientId,
    @FullscriptPatientId,
    @FullscriptMetadataId,
    @FullscriptEmail,
    @FullscriptFirstName,
    @FullscriptLastName,
    @Environment,
    @ClinicId,
    @LastSyncedDateTime,
    @Active,
    @CreatedDateTime,
    @UpdatedDateTime
);

SELECT CONVERT(int, SCOPE_IDENTITY());";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, map);

                    int newId = Convert.ToInt32(command.ExecuteScalar());
                    map.FullscriptPatientMapId = newId;

                    return newId;
                }
            }
        }

        public void Update(FullscriptPatientMapDto map)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }

            if (!map.FullscriptPatientMapId.HasValue)
            {
                throw new InvalidOperationException("FullscriptPatientMapId is required for update.");
            }

            map.UpdatedDateTime = DateTime.UtcNow;

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
UPDATE dbo.FullscriptPatientMap
SET
    FullscriptPatientMapGuid = @FullscriptPatientMapGuid,
    PatientId = @PatientId,
    FullscriptPatientId = @FullscriptPatientId,
    FullscriptMetadataId = @FullscriptMetadataId,
    FullscriptEmail = @FullscriptEmail,
    FullscriptFirstName = @FullscriptFirstName,
    FullscriptLastName = @FullscriptLastName,
    Environment = @Environment,
    ClinicId = @ClinicId,
    LastSyncedDateTime = @LastSyncedDateTime,
    Active = @Active,
    CreatedDateTime = @CreatedDateTime,
    UpdatedDateTime = @UpdatedDateTime
WHERE FullscriptPatientMapId = @FullscriptPatientMapId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    AddParameters(command, map);
                    command.Parameters.Add("@FullscriptPatientMapId", SqlDbType.Int).Value =
                        map.FullscriptPatientMapId.Value;

                    int affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("Fullscript patient map was not found.");
                    }
                }
            }
        }

        public void MarkInactive(int fullscriptPatientMapId)
        {
            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
UPDATE dbo.FullscriptPatientMap
SET
    Active = 0,
    UpdatedDateTime = SYSUTCDATETIME()
WHERE FullscriptPatientMapId = @FullscriptPatientMapId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@FullscriptPatientMapId", SqlDbType.Int).Value = fullscriptPatientMapId;

                    int affected = command.ExecuteNonQuery();

                    if (affected == 0)
                    {
                        throw new InvalidOperationException("Fullscript patient map was not found.");
                    }
                }
            }
        }

        private static FullscriptPatientMapDto MapReader(SqlDataReader reader)
        {
            return new FullscriptPatientMapDto
            {
                FullscriptPatientMapId = GetNullableInt(reader, "FullscriptPatientMapId"),
                FullscriptPatientMapGuid = GetGuid(reader, "FullscriptPatientMapGuid"),
                PatientId = GetInt(reader, "PatientId"),
                FullscriptPatientId = GetNullableString(reader, "FullscriptPatientId"),
                FullscriptMetadataId = GetNullableString(reader, "FullscriptMetadataId"),
                FullscriptEmail = GetNullableString(reader, "FullscriptEmail"),
                FullscriptFirstName = GetNullableString(reader, "FullscriptFirstName"),
                FullscriptLastName = GetNullableString(reader, "FullscriptLastName"),
                Environment = GetNullableString(reader, "Environment"),
                ClinicId = GetNullableString(reader, "ClinicId"),
                LastSyncedDateTime = GetNullableDateTime(reader, "LastSyncedDateTime"),
                Active = GetBoolean(reader, "Active"),
                CreatedDateTime = GetDateTime(reader, "CreatedDateTime"),
                UpdatedDateTime = GetNullableDateTime(reader, "UpdatedDateTime")
            };
        }

        private static void AddParameters(SqlCommand command, FullscriptPatientMapDto map)
        {
            command.Parameters.Add("@FullscriptPatientMapGuid", SqlDbType.UniqueIdentifier).Value =
                map.FullscriptPatientMapGuid;

            command.Parameters.Add("@PatientId", SqlDbType.Int).Value = map.PatientId;

            AddNullableNVarChar(command, "@FullscriptPatientId", map.FullscriptPatientId, 100);
            AddNullableNVarChar(command, "@FullscriptMetadataId", map.FullscriptMetadataId, 100);
            AddNullableNVarChar(command, "@FullscriptEmail", map.FullscriptEmail, 300);
            AddNullableNVarChar(command, "@FullscriptFirstName", map.FullscriptFirstName, 100);
            AddNullableNVarChar(command, "@FullscriptLastName", map.FullscriptLastName, 100);
            AddNullableNVarChar(command, "@Environment", map.Environment, 50);
            AddNullableNVarChar(command, "@ClinicId", map.ClinicId, 100);
            AddNullableDateTime(command, "@LastSyncedDateTime", map.LastSyncedDateTime);

            command.Parameters.Add("@Active", SqlDbType.Bit).Value = map.Active;

            command.Parameters.Add("@CreatedDateTime", SqlDbType.DateTime2).Value = map.CreatedDateTime;
            AddNullableDateTime(command, "@UpdatedDateTime", map.UpdatedDateTime);
        }

        private static void AddNullableNVarChar(SqlCommand command, string parameterName, string value, int size)
        {
            SqlParameter parameter = command.Parameters.Add(parameterName, SqlDbType.NVarChar, size);
            parameter.Value = string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value;
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