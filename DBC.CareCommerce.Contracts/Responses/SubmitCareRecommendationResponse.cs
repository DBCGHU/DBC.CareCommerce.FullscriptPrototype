using System.Collections.Generic;
using DBC.CareCommerce.Contracts.Models;

namespace DBC.CareCommerce.Contracts.Responses
{
    public class SubmitCareRecommendationResponse
    {
        public SubmitCareRecommendationResponse()
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

        public string PmAction { get; set; }
        public string FullscriptAction { get; set; }

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

        public void ApplyCreateCareItemResponse(CreateCareItemResponse response)
        {
            if (response == null)
            {
                AddError("CreateCareItem response was not created.");
                return;
            }

            Success = response.Success;

            CareItemId = response.CareItemId;
            CatalogItemId = response.CatalogItemId;
            PendingChargeId = response.PendingChargeId;
            FullscriptTransactionId = response.FullscriptTransactionId;

            CareItem = response.CareItem;
            CatalogItem = response.CatalogItem;
            PendingCharge = response.PendingCharge;
            FullscriptTransaction = response.FullscriptTransaction;
            WorkflowDecision = response.WorkflowDecision;

            if (PendingChargeId.HasValue)
            {
                PmAction = "CreatePendingCharge";
            }
            else
            {
                PmAction = "NoPmAction";
            }

            if (FullscriptTransactionId.HasValue)
            {
                FullscriptAction = "QueueFullscriptTransaction";
            }
            else
            {
                FullscriptAction = "NoFullscriptAction";
            }

            foreach (string message in response.Messages)
            {
                AddMessage(message);
            }

            foreach (string warning in response.Warnings)
            {
                AddWarning(warning);
            }

            foreach (string error in response.Errors)
            {
                AddError(error);
            }

            Success = Errors.Count == 0;
        }
    }
}