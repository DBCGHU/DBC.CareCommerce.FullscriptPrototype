using System;
using DBC.CareCommerce.Contracts.Enums;

namespace DBC.CareCommerce.Contracts.Models
{
    public class PendingChargeDto
    {
        public PendingChargeDto()
        {
            PendingChargeGuid = Guid.NewGuid();
            Quantity = 1m;
            BillingAction = BillingAction.CreatePendingCharge;
            InventoryAction = InventoryAction.None;
            FulfillmentSource = FulfillmentSource.None;
            Status = PendingChargeStatus.Pending;
            Active = true;
            CreatedDateTime = DateTime.UtcNow;
        }

        public int? PendingChargeId { get; set; }
        public Guid PendingChargeGuid { get; set; }

        public int? CareItemId { get; set; }

        public int PatientId { get; set; }
        public int PatientCaseId { get; set; }
        public int? ProviderId { get; set; }

        public int? CatalogItemId { get; set; }
        public int? FeeId { get; set; }
        public int? ProductId { get; set; }

        public string Description { get; set; }

        public decimal Quantity { get; set; }
        public decimal? UnitAmount { get; set; }
        public decimal? TotalAmount { get; set; }

        public BillingAction BillingAction { get; set; }
        public InventoryAction InventoryAction { get; set; }
        public FulfillmentSource FulfillmentSource { get; set; }

        public PendingChargeStatus Status { get; set; }

        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedDateTime { get; set; }

        public int? RejectedByUserId { get; set; }
        public DateTime? RejectedDateTime { get; set; }
        public string RejectionReason { get; set; }

        public DateTime? PostedDateTime { get; set; }
        public int? PostingId { get; set; }

        public string ErrorMessage { get; set; }

        public int? CreatedByUserId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? UpdatedDateTime { get; set; }

        public bool Active { get; set; }

        public void CalculateTotal()
        {
            if (UnitAmount.HasValue)
            {
                TotalAmount = Quantity * UnitAmount.Value;
            }
            else
            {
                TotalAmount = null;
            }
        }

        public bool CanPost()
        {
            return Status == PendingChargeStatus.Approved &&
                   PatientId > 0 &&
                   PatientCaseId > 0 &&
                   FeeId.HasValue;
        }
    }
}