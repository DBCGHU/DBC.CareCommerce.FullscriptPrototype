using System;

namespace DBC.CareCommerce.Contracts.Models
{
    public class InventoryAvailabilityDto
    {
        public InventoryAvailabilityDto()
        {
            StockStatus = "Unknown";
            LastUpdatedDateTime = DateTime.UtcNow;
        }

        public int? InventoryAvailabilityId { get; set; }

        public int CatalogItemId { get; set; }
        public int? ProductId { get; set; }
        public int? LocationId { get; set; }

        public decimal OnHand { get; set; }
        public decimal Reserved { get; set; }
        public decimal? ReorderLevel { get; set; }

        public string StockStatus { get; set; }
        public DateTime LastUpdatedDateTime { get; set; }

        public decimal Available
        {
            get { return OnHand - Reserved; }
        }

        public bool IsInStock(decimal requestedQuantity)
        {
            return Available >= requestedQuantity;
        }

        public bool IsInStock()
        {
            return IsInStock(1m);
        }

        public bool IsLowStock()
        {
            if (!ReorderLevel.HasValue)
            {
                return false;
            }

            return Available <= ReorderLevel.Value;
        }

        public string GetCalculatedStockStatus()
        {
            if (Available <= 0m)
            {
                return "OutOfStock";
            }

            if (IsLowStock())
            {
                return "LowStock";
            }

            return "InStock";
        }
    }
}