using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Repositories
{
    public interface IFullscriptTransactionRepository
    {
        FullscriptTransactionDto GetById(int fullscriptTransactionId);

        IList<FullscriptTransactionDto> GetByCareItemId(int careItemId);

        IList<FullscriptTransactionDto> GetPendingTransactions();

        int Insert(FullscriptTransactionDto transaction);

        void Update(FullscriptTransactionDto transaction);

        void MarkSent(int fullscriptTransactionId, string externalReferenceId);

        void MarkFailed(int fullscriptTransactionId, string errorMessage);
    }
}
