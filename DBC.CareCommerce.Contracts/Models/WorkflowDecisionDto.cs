using System;
using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Enums;

namespace DBC.CareCommerce.Contracts.Models
{
    public class WorkflowDecisionDto
    {
        public WorkflowDecisionDto()
        {
            Messages = new List<string>();
            Warnings = new List<string>();
            Errors = new List<string>();
            CreatedDateTime = DateTime.UtcNow;
        }

        public CareItemDto CareItem { get; set; }
        public CatalogItemDto CatalogItem { get; set; }
        public PendingChargeDto PendingCharge { get; set; }
        public FullscriptTransactionDto FullscriptTransaction { get; set; }
        public InventoryAvailabilityDto InventoryAvailability { get; set; }

        public bool ShouldCreatePendingCharge { get; set; }
        public bool ShouldCreateFullscriptTransaction { get; set; }
        public bool ShouldAffectLocalInventory { get; set; }
        public bool ShouldCreatePostingImmediately { get; set; }
        public bool IsDocumentationOnly { get; set; }

        public FulfillmentSource FulfillmentSource { get; set; }
        public BillingAction BillingAction { get; set; }
        public InventoryAction InventoryAction { get; set; }

        public List<string> Messages { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Errors { get; set; }

        public DateTime CreatedDateTime { get; set; }

        public bool IsSuccessful
        {
            get { return Errors.Count == 0; }
        }

        public void AddMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Messages.Add(message);
            }
        }

        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Warnings.Add(message);
            }
        }

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Errors.Add(message);
            }
        }
    }
}