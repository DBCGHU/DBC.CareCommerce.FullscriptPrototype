using DBC.CareCommerce.Application.Services;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Data.DataAccess;
using DBC.CareCommerce.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DBC.CareCommerce.WindowsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly CareCommerceServiceSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly FullscriptTransactionDispatcherService _dispatcherService;

        public Worker(
            ILogger<Worker> logger,
            IOptions<CareCommerceServiceSettings> settings,
            IConfiguration configuration,
            FullscriptTransactionDispatcherService dispatcherService)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (dispatcherService == null)
            {
                throw new ArgumentNullException("dispatcherService");
            }

            _logger = logger;
            _settings = settings.Value ?? new CareCommerceServiceSettings();
            _configuration = configuration;
            _dispatcherService = dispatcherService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DBC Care Commerce Windows Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DispatchReadyFullscriptTransactions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while dispatching ready Fullscript transactions.");
                }

                int intervalSeconds = GetDispatchIntervalSeconds();

                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
            }

            _logger.LogInformation("DBC Care Commerce Windows Service stopped.");
        }

        private void DispatchReadyFullscriptTransactions()
        {
            var dispatchedTransactions = _dispatcherService.DispatchReadyTransactions();

            if (dispatchedTransactions.Count == 0)
            {
                _logger.LogInformation("No ReadyToSend Fullscript transactions found.");
                return;
            }

            _logger.LogInformation(
                "Dispatched {Count} ReadyToSend Fullscript transaction(s).",
                dispatchedTransactions.Count);

            foreach (FullscriptTransactionDto transaction in dispatchedTransactions)
            {
                _logger.LogInformation(
                    "FullscriptTransactionID {FullscriptTransactionId} dispatched with Status {Status}, TreatmentPlanID {TreatmentPlanId}, ErrorMessage {ErrorMessage}.",
                    transaction.FullscriptTransactionId,
                    transaction.Status,
                    transaction.FullscriptTreatmentPlanId,
                    transaction.ErrorMessage);
            }
        }

        private int GetDispatchIntervalSeconds()
        {
            if (_settings.FullscriptDispatchIntervalSeconds > 0)
            {
                return _settings.FullscriptDispatchIntervalSeconds;
            }

            return 60;
        }
    }
}