using System;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;
using DBC.Integrations.Fullscript.Models;

namespace DBC.Integrations.Fullscript.Services
{
    public class FullscriptConnectionService
    {
        private readonly IFullscriptConnectionRepository _connectionRepository;

        public FullscriptConnectionService(IFullscriptConnectionRepository connectionRepository)
        {
            if (connectionRepository == null)
            {
                throw new ArgumentNullException("connectionRepository");
            }

            _connectionRepository = connectionRepository;
        }

        public FullscriptConnectionDto SaveConnectionFromTokenAndClinic(
            FullscriptTokenResponse token,
            string environment,
            string clientId,
            string clinicId,
            string clinicName,
            string dispensaryUrl,
            string integrationId,
            DateTime? integrationActivatedAt)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            if (!token.HasAccessToken())
            {
                throw new InvalidOperationException("Token response does not contain an access token.");
            }

            if (string.IsNullOrWhiteSpace(environment))
            {
                environment = "UsSandbox";
            }

            FullscriptConnectionDto connection = null;

            if (!string.IsNullOrWhiteSpace(clinicId))
            {
                connection = _connectionRepository.GetActiveByEnvironmentAndClinic(environment, clinicId);
            }

            if (connection == null)
            {
                connection = new FullscriptConnectionDto
                {
                    Environment = environment,
                    ClinicId = clinicId,
                    ClinicName = clinicName,
                    ClientId = clientId,
                    DispensaryUrl = dispensaryUrl,
                    IntegrationId = integrationId,
                    IntegrationActivatedAt = integrationActivatedAt,
                    Status = "Active",
                    Active = true,
                    CreatedDateTime = DateTime.UtcNow
                };

                ApplyTokenToConnection(connection, token);

                var id = _connectionRepository.Insert(connection);
                connection.FullscriptConnectionId = id;

                return connection;
            }

            connection.ClinicName = clinicName;
            connection.ClientId = clientId;
            connection.DispensaryUrl = dispensaryUrl;
            connection.IntegrationId = integrationId;
            connection.IntegrationActivatedAt = integrationActivatedAt;
            connection.Status = "Active";
            connection.Active = true;
            connection.UpdatedDateTime = DateTime.UtcNow;

            ApplyTokenToConnection(connection, token);

            _connectionRepository.Update(connection);

            return connection;
        }

        public FullscriptConnectionDto GetActiveConnection(string environment, string clinicId)
        {
            if (string.IsNullOrWhiteSpace(environment))
            {
                environment = "UsSandbox";
            }

            return _connectionRepository.GetActiveByEnvironmentAndClinic(environment, clinicId);
        }

        private static void ApplyTokenToConnection(FullscriptConnectionDto connection, FullscriptTokenResponse token)
        {
            /*
             * Prototype note:
             * These fields are named AccessTokenEncrypted and RefreshTokenEncrypted
             * because production code should encrypt tokens before persistence.
             * This prototype currently stores the raw token value in those fields.
             * Do not use this as production token storage.
             */

            connection.AccessTokenEncrypted = token.AccessToken;
            connection.RefreshTokenEncrypted = token.RefreshToken;
            connection.TokenType = token.TokenType;
            connection.Scope = token.Scope;
            connection.TokenReceivedDateTime = token.ReceivedDateTimeUtc;
            connection.TokenExpiresAtDateTime = token.ExpiresAtUtc;
        }
    }
}