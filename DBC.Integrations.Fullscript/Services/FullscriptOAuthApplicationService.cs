using System;
using System.Threading.Tasks;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Data.Repositories;
using DBC.Integrations.Fullscript.Client;
using DBC.Integrations.Fullscript.Configuration;
using DBC.Integrations.Fullscript.Models;
using DBC.Integrations.Fullscript.OAuth;
using DBC.CareCommerce.Data.Security;

namespace DBC.Integrations.Fullscript.Services
{
    public class FullscriptOAuthApplicationService
    {
        private readonly IFullscriptConnectionRepository _connectionRepository;
        private readonly ITokenEncryptionService _tokenEncryptionService;

        public FullscriptOAuthApplicationService(IFullscriptConnectionRepository connectionRepository, ITokenEncryptionService tokenEncryptionService)
        {
            if (connectionRepository == null)
            {
                throw new ArgumentNullException("connectionRepository");
            }

            if (tokenEncryptionService == null)
            {
                throw new ArgumentNullException("tokenEncryptionService");
            }

            _connectionRepository = connectionRepository;
            _tokenEncryptionService = tokenEncryptionService;
        }

        public async Task<FullscriptConnectionDto> CompleteAuthorizationAsync(
            FullscriptConfiguration configuration,
            string authorizationCode)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            configuration.ValidateForTokenExchange();

            if (string.IsNullOrWhiteSpace(authorizationCode))
            {
                throw new ArgumentException("Authorization code is required.", "authorizationCode");
            }

            var tokenClient = new FullscriptTokenClient();

            var token = await tokenClient.ExchangeAuthorizationCodeAsync(
                configuration,
                authorizationCode).ConfigureAwait(false);

            var apiClient = new FullscriptApiClient(configuration);

            var clinicJson = await apiClient.GetClinicRawJsonAsync(
                token.AccessToken).ConfigureAwait(false);

            var clinic = FullscriptClinicResponseParser.Parse(clinicJson);

            if (clinic == null || !clinic.HasClinicId())
            {
                throw new InvalidOperationException("Fullscript clinic response did not include a clinic ID.");
            }

            var connectionService = new FullscriptConnectionService(_connectionRepository, _tokenEncryptionService);

            var connection = connectionService.SaveConnectionFromTokenAndClinic(
                token,
                configuration.Environment.ToString(),
                configuration.ClientId,
                clinic.ClinicId,
                clinic.Name,
                clinic.DispensaryUrl,
                clinic.IntegrationId,
                clinic.IntegrationActivatedAt);

            return connection;
        }

        public async Task<FullscriptConnectionDto> RefreshConnectionAsync(
            FullscriptConfiguration configuration,
            FullscriptConnectionDto connection)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            configuration.ValidateForTokenExchange();

            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (!connection.HasRefreshToken())
            {
                throw new InvalidOperationException("Fullscript connection does not have a refresh token.");
            }

            var tokenClient = new FullscriptTokenClient();

            var refreshedToken = await tokenClient.RefreshAccessTokenAsync(
                configuration,
                _tokenEncryptionService.Decrypt(connection.RefreshTokenEncrypted)).ConfigureAwait(false);

            var connectionService = new FullscriptConnectionService(_connectionRepository, _tokenEncryptionService);

            var refreshedConnection = connectionService.SaveConnectionFromTokenAndClinic(
                refreshedToken,
                connection.Environment,
                configuration.ClientId,
                connection.ClinicId,
                connection.ClinicName,
                connection.DispensaryUrl,
                connection.IntegrationId,
                connection.IntegrationActivatedAt);

            refreshedConnection.LastRefreshDateTime = DateTime.UtcNow;
            _connectionRepository.Update(refreshedConnection);

            return refreshedConnection;
        }

        public FullscriptConnectionDto GetActiveConnection(
            string environment,
            string clinicId)
        {
            var connectionService = new FullscriptConnectionService(_connectionRepository, _tokenEncryptionService);

            return connectionService.GetActiveConnection(
                environment,
                clinicId);
        }
    }
}