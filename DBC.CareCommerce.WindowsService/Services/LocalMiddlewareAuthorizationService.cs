using System;

namespace DBC.CareCommerce.WindowsService.Services
{
    public sealed class LocalMiddlewareAuthorizationService
    {
        public const string HeaderName = "X-DBC-CareCommerce-Token";

        public bool IsAuthorized(string suppliedToken)
        {
            string expectedToken =
                Environment.GetEnvironmentVariable("DBC_CARECOMMERCE_LOCAL_TOKEN") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(expectedToken))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(suppliedToken))
            {
                return false;
            }

            return string.Equals(
                expectedToken,
                suppliedToken,
                StringComparison.Ordinal);
        }
    }
}