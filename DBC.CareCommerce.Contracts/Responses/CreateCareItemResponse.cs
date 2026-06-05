using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Responses
{
    public class CreateCareItemResponse
    {
        public CreateCareItemResponse()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
            Messages = new List<string>();
        }

        public bool Success { get; set; }

        public int? CareItemId { get; set; }
        public int? CatalogItemId { get; set; }
        public int? PendingChargeId { get; set; }
        public int? FullscriptTransactionId { get; set; }

        public CareItemDto CareItem { get; set; }
        public CatalogItemDto CatalogItem { get; set; }
        public PendingChargeDto PendingCharge { get; set; }
        public FullscriptTransactionDto FullscriptTransaction { get; set; }
        public WorkflowDecisionDto WorkflowDecision { get; set; }

        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> Messages { get; set; }

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Errors.Add(message);
                Success = false;
            }
        }

        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Warnings.Add(message);
            }
        }

        public void AddMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Messages.Add(message);
            }
        }

        public void ApplyWorkflowDecision(WorkflowDecisionDto decision)
        {
            WorkflowDecision = decision;

            if (decision == null)
            {
                AddError("Workflow decision was not created.");
                return;
            }

            CareItem = decision.CareItem;
            CatalogItem = decision.CatalogItem;
            PendingCharge = decision.PendingCharge;
            FullscriptTransaction = decision.FullscriptTransaction;

            if (CareItem != null)
            {
                CareItemId = CareItem.CareItemId;
            }

            if (CatalogItem != null)
            {
                CatalogItemId = CatalogItem.CatalogItemId;
            }

            if (PendingCharge != null)
            {
                PendingChargeId = PendingCharge.PendingChargeId;
            }

            if (FullscriptTransaction != null)
            {
                FullscriptTransactionId = FullscriptTransaction.FullscriptTransactionId;
            }

            foreach (var message in decision.Messages)
            {
                AddMessage(message);
            }

            foreach (var warning in decision.Warnings)
            {
                AddWarning(warning);
            }

            foreach (var error in decision.Errors)
            {
                AddError(error);
            }

            Success = Errors.Count == 0;
        }
    }
}