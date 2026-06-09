using System;
using System.Data;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Data.DataAccess;
using Microsoft.Data.SqlClient;

namespace DBC.CareCommerce.Data.Repositories
{
    public sealed class SqlPatientProfileRepository : IPatientProfileRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public SqlPatientProfileRepository(SqlConnectionFactory connectionFactory)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException("connectionFactory");
            }

            _connectionFactory = connectionFactory;
        }

        public FullscriptPatientProfileDto GetByPatientId(int patientId)
        {
            if (patientId <= 0)
            {
                throw new ArgumentException("A valid patient ID is required.", "patientId");
            }

            using (SqlConnection connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                const string sql = @"
SELECT TOP 1
    ID,
    HomeEmail,
    NameFirst,
    NameLast,
    DOB
FROM dbo.Patient
WHERE ID = @PatientId;";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = patientId;

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

        private static FullscriptPatientProfileDto MapReader(SqlDataReader reader)
        {
            return new FullscriptPatientProfileDto
            {
                PatientId = GetInt(reader, "ID"),
                Email = GetNullableString(reader, "HomeEmail"),
                FirstName = GetNullableString(reader, "NameFirst"),
                LastName = GetNullableString(reader, "NameLast"),
                DateOfBirth = GetNullableDateTime(reader, "DOB")
            };
        }

        private static int GetInt(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(value);
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

        private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];

            if (value == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(value);
        }
    }
}