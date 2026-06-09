using System;
using System.Threading;
using System.Threading.Tasks;
using DBC.CareCommerce.Application.Services;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Data.DataAccess;
using DBC.CareCommerce.Data.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DBC.CareCommerce.WindowsService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            _logger = logger;
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

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }

            _logger.LogInformation("DBC Care Commerce Windows Service stopped.");
        }

        private void DispatchReadyFullscriptTransactions()
        {
            string connectionString = Environment.GetEnvironmentVariable("DBC_CARECOMMERCE_SQL_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogWarning("Environment variable DBC_CARECOMMERCE_SQL_CONNECTION is not set. Fullscript transaction dispatch skipped.");
                return;
            }

            SqlConnectionFactory sqlConnectionFactory = new SqlConnectionFactory(connectionString);

            IFullscriptTransactionRepository fullscriptTransactionRepository =
                new SqlFullscriptTransactionRepository(sqlConnectionFactory);

            FullscriptTransactionDispatcherService dispatcherService =
                new FullscriptTransactionDispatcherService(fullscriptTransactionRepository);

            var dispatchedTransactions = dispatcherService.DispatchReadyTransactions();

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
    }
}