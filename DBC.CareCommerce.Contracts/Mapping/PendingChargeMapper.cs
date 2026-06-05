using System;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Mapping
{
    public class PendingChargeMapper
    {
        public PendingChargeDto FromCareItem(CareItemDto item, CatalogItemDto catalogItem)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (catalogItem == null)
            {
                throw new ArgumentNullException("catalogItem");
            }

            var charge = new PendingChargeDto
            {
                CareItemId = item.CareItemId,
                PatientId = item.PatientId,
                PatientCaseId = item.PatientCaseId.HasValue ? item.PatientCaseId.Value : 0,
                ProviderId = item.ProviderId,
                CatalogItemId = catalogItem.CatalogItemId,
                FeeId = catalogItem.FeeId,
                ProductId = catalogItem.ProductId,
                Description = catalogItem.DisplayName,
                Quantity = item.QuantityDispensed.HasValue ? item.QuantityDispensed.Value : 1m,
                UnitAmount = catalogItem.DefaultChargeAmount,
                BillingAction = item.BillingIntent,
                InventoryAction = item.InventoryIntent,
                FulfillmentSource = item.FulfillmentSource
            };

            charge.CalculateTotal();

            return charge;
        }
    }
}