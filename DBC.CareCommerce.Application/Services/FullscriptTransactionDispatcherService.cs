using System;
using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;

namespace DBC.CareCommerce.Application.Services
{
    public sealed class FullscriptTransactionDispatcherService
    {
        private readonly IFullscriptTransactionRepository _fullscriptTransactionRepository;

        public FullscriptTransactionDispatcherService(
            IFullscriptTransactionRepository fullscriptTransactionRepository)
        {
            if (fullscriptTransactionRepository == null)
            {
                throw new ArgumentNullException("fullscriptTransactionRepository");
            }

            _fullscriptTransactionRepository = fullscriptTransactionRepository;
        }

        public IList<FullscriptTransactionDto> DispatchReadyTransactions()
        {
            IList<FullscriptTransactionDto> readyTransactions =
                _fullscriptTransactionRepository.GetPendingTransactions();

            foreach (FullscriptTransactionDto transaction in readyTransactions)
            {
                DispatchReadyTransaction(transaction);
            }

            return readyTransactions;
        }

        private void DispatchReadyTransaction(FullscriptTransactionDto transaction)
        {
            if (transaction == null)
            {
                return;
            }

            if (!transaction.FullscriptTransactionId.HasValue)
            {
                return;
            }

            string validationError = ValidateReadyTransaction(transaction);

            if (!string.IsNullOrWhiteSpace(validationError))
            {
                _fullscriptTransactionRepository.MarkFailed(
                    transaction.FullscriptTransactionId.Value,
                    validationError);

                transaction.Status = "Failed";
                transaction.ErrorMessage = validationError;

                return;
            }

            string fakeTreatmentPlanId =
                "stub-treatment-plan-" + transaction.FullscriptTransactionId.Value;

            _fullscriptTransactionRepository.MarkSent(
                transaction.FullscriptTransactionId.Value,
                fakeTreatmentPlanId);

            transaction.Status = "Sent";
            transaction.FullscriptTreatmentPlanId = fakeTreatmentPlanId;
        }

        private static string ValidateReadyTransaction(FullscriptTransactionDto transaction)
        {
            if (transaction.PatientId <= 0)
            {
                return "PatientId is required before dispatching Fullscript transaction.";
            }

            if (!transaction.CareItemId.HasValue)
            {
                return "CareItemId is required before dispatching Fullscript transaction.";
            }

            if (!transaction.CatalogItemId.HasValue)
            {
                return "CatalogItemId is required before dispatching Fullscript transaction.";
            }

            if (string.IsNullOrWhiteSpace(transaction.FullscriptProductId))
            {
                return "FullscriptProductId is required before dispatching Fullscript transaction.";
            }

            if (string.IsNullOrWhiteSpace(transaction.FullscriptVariantId))
            {
                return "FullscriptVariantId is required before dispatching Fullscript transaction.";
            }

            return null;
        }
    }
}