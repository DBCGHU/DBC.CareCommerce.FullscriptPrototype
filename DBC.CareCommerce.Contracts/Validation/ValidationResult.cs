using System.Collections.Generic;

namespace DBC.CareCommerce.Contracts.Validation
{
    public class ValidationResult
    {
        public ValidationResult()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
        }

        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }

        public bool IsValid
        {
            get { return Errors.Count == 0; }
        }

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Errors.Add(message);
            }
        }

        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Warnings.Add(message);
            }
        }
    }
}