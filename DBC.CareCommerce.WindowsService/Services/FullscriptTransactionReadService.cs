using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;

namespace DBC.CareCommerce.WindowsService.Services
{
    public sealed class FullscriptTransactionReadService
    {
        private readonly IFullscriptTransactionRepository _fullscriptTransactionRepository;

        public FullscriptTransactionReadService(
            IFullscriptTransactionRepository fullscriptTransactionRepository)
        {
            _fullscriptTransactionRepository = fullscriptTransactionRepository;
        }

        public object GetByFullscriptTransactionId(int fullscriptTransactionId)
        {
            FullscriptTransactionDto? transaction =
                _fullscriptTransactionRepository.GetById(fullscriptTransactionId);

            if (transaction == null)
            {
                return new
                {
                    success = false,
                    fullscriptTransactionId = fullscriptTransactionId,
                    transaction = (object)null,
                    errors = new List<string>
                    {
                        "Fullscript transaction was not found."
                    },
                    warnings = new List<string>(),
                    messages = new List<string>()
                };
            }

            return new
            {
                success = true,
                fullscriptTransactionId = fullscriptTransactionId,
                transaction = transaction,
                errors = new List<string>(),
                warnings = new List<string>(),
                messages = new List<string>
                {
                    "Fullscript transaction read completed."
                }
            };
        }

        public object GetReadyTransactions()
        {
            IList<FullscriptTransactionDto> transactions =
                _fullscriptTransactionRepository.GetPendingTransactions();

            if (transactions == null)
            {
                transactions = new List<FullscriptTransactionDto>();
            }

            return new
            {
                success = true,
                count = transactions.Count,
                transactions = transactions,
                errors = new List<string>(),
                warnings = new List<string>(),
                messages = new List<string>
        {
            "Ready Fullscript transaction read completed."
        }
            };
        }
    }
}