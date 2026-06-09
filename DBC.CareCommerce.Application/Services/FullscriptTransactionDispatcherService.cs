using System;
using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Contracts.Services;

namespace DBC.CareCommerce.Application.Services
{
    public sealed class FullscriptTransactionDispatcherService
    {
        private readonly IFullscriptTransactionRepository _fullscriptTransactionRepository;
        private readonly IFullscriptApiClient _fullscriptApiClient;

        public FullscriptTransactionDispatcherService(
            IFullscriptTransactionRepository fullscriptTransactionRepository,
            IFullscriptApiClient fullscriptApiClient)
        {
            if (fullscriptTransactionRepository == null)
            {
                throw new ArgumentNullException("fullscriptTransactionRepository");
            }

            if (fullscriptApiClient == null)
            {
                throw new ArgumentNullException("fullscriptApiClient");
            }

            _fullscriptTransactionRepository = fullscriptTransactionRepository;
            _fullscriptApiClient = fullscriptApiClient;
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

            FullscriptDispatchResultDto dispatchResult =
                _fullscriptApiClient.DispatchTreatmentPlan(transaction);

            if (dispatchResult == null)
            {
                _fullscriptTransactionRepository.MarkFailed(
                    transaction.FullscriptTransactionId.Value,
                    "Fullscript dispatch result was not returned.");

                transaction.Status = "Failed";
                transaction.ErrorMessage = "Fullscript dispatch result was not returned.";

                return;
            }

            if (!dispatchResult.Success)
            {
                string errorMessage = dispatchResult.ErrorMessage;

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "Fullscript dispatch failed.";
                }

                _fullscriptTransactionRepository.MarkFailed(
                    transaction.FullscriptTransactionId.Value,
                    errorMessage);

                transaction.Status = "Failed";
                transaction.ErrorMessage = errorMessage;

                return;
            }

            _fullscriptTransactionRepository.MarkSent(
                transaction.FullscriptTransactionId.Value,
                dispatchResult.ExternalReferenceId);

            transaction.Status = "Sent";
            transaction.FullscriptTreatmentPlanId = dispatchResult.ExternalReferenceId;
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