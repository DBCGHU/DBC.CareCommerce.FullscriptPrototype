using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Data.Repositories;

namespace DBC.CareCommerce.WindowsService.Services
{
    public sealed class CareCommerceRecommendationReadService
    {
        private readonly ICareItemRepository _careItemRepository;
        private readonly IPendingChargeRepository _pendingChargeRepository;
        private readonly IFullscriptTransactionRepository _fullscriptTransactionRepository;

        public CareCommerceRecommendationReadService(
            ICareItemRepository careItemRepository,
            IPendingChargeRepository pendingChargeRepository,
            IFullscriptTransactionRepository fullscriptTransactionRepository)
        {
            _careItemRepository = careItemRepository;
            _pendingChargeRepository = pendingChargeRepository;
            _fullscriptTransactionRepository = fullscriptTransactionRepository;
        }

        public object GetByCareItemId(int careItemId)
        {
            CareItemDto careItem = _careItemRepository.GetById(careItemId);

            if (careItem == null)
            {
                return new
                {
                    success = false,
                    careItemId = careItemId,
                    errors = new List<string>
                    {
                        "Care item was not found."
                    }
                };
            }

            IList<PendingChargeDto> pendingCharges =
                _pendingChargeRepository.GetByCareItemId(careItemId);

            IList<FullscriptTransactionDto> fullscriptTransactions =
                _fullscriptTransactionRepository.GetByCareItemId(careItemId);

            int? pendingChargeId = null;
            if (pendingCharges != null && pendingCharges.Count > 0)
            {
                pendingChargeId = pendingCharges[0].PendingChargeId;
            }

            int? fullscriptTransactionId = null;
            if (fullscriptTransactions != null && fullscriptTransactions.Count > 0)
            {
                fullscriptTransactionId = fullscriptTransactions[0].FullscriptTransactionId;
            }

            return new
            {
                success = true,
                careItemId = careItem.CareItemId,
                pendingChargeId = pendingChargeId,
                fullscriptTransactionId = fullscriptTransactionId,
                careItem = careItem,
                pendingCharges = pendingCharges,
                fullscriptTransactions = fullscriptTransactions,
                errors = new List<string>(),
                warnings = new List<string>(),
                messages = new List<string>
                {
                    "Care recommendation read completed."
                }
            };
        }
    }
}