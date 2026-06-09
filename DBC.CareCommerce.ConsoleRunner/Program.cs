using DBC.CareCommerce.Application.Services;
using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Mapping;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Contracts.Requests;
using DBC.CareCommerce.Contracts.Responses;
using DBC.CareCommerce.Data.DataAccess;
using DBC.CareCommerce.Data.InMemory;
using DBC.CareCommerce.Data.Repositories;
using DBC.CareCommerce.Data.Security;
using DBC.Integrations.Fullscript.Client;
using DBC.Integrations.Fullscript.Configuration;
using DBC.Integrations.Fullscript.OAuth;
using DBC.Integrations.Fullscript.Services;
using System;
using System.Diagnostics;

namespace DBC.CareCommerce.ConsoleRunner
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("======================================");
                Console.WriteLine("DBC Care Commerce Console Runner");
                Console.WriteLine();
                Console.WriteLine("Select demo:");
                Console.WriteLine("1. Run Fullscript OAuth/API demo");
                Console.WriteLine("2. Run Care Commerce workflow demo only");
                Console.WriteLine("3. Run both");
                Console.WriteLine();

                Console.Write("Enter selection 1, 2, or 3: ");
                string selection = Console.ReadLine();

                Console.WriteLine();

                if (selection == "1")
                {
                    await RunFullscriptAuthorizeUrlDemo();

                    Console.WriteLine();
                    Console.WriteLine("Fullscript OAuth/API demo completed.");
                }
                else if (selection == "2")
                {
                    RunApplicationServiceDemo();

                    Console.WriteLine();
                    Console.WriteLine("Care Commerce application-service demo completed.");
                }
                else if (selection == "3")
                {
                    await RunFullscriptAuthorizeUrlDemo();

                    RunApplicationServiceDemo();

                    Console.WriteLine();
                    Console.WriteLine("Fullscript OAuth/API demo and Care Commerce application-service demo completed.");
                }
                else
                {
                    Console.WriteLine("Invalid selection. No demo was run.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Demo failed:");
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        private static void RunApplicationServiceDemo()
        {
            var connectionString = Environment.GetEnvironmentVariable("DBC_CARECOMMERCE_SQL_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("Missing SQL connection string.");
                Console.WriteLine("Set environment variable DBC_CARECOMMERCE_SQL_CONNECTION before running the application-service demo.");
                return;
            }

            var sqlConnectionFactory = new SqlConnectionFactory(connectionString);

            ICatalogItemRepository catalogRepository = new SqlCatalogItemRepository(sqlConnectionFactory);
            ICareItemRepository careItemRepository = new SqlCareItemRepository(sqlConnectionFactory);
            IPendingChargeRepository pendingChargeRepository = new SqlPendingChargeRepository(sqlConnectionFactory);
            IFullscriptTransactionRepository fullscriptTransactionRepository = new SqlFullscriptTransactionRepository(sqlConnectionFactory);
            IFullscriptPatientMapRepository fullscriptPatientMapRepository = new InMemoryFullscriptPatientMapRepository();

            var catalogMapper = new CatalogItemMapper();

            var localVitaminD = catalogMapper.FromProduct(
                productId: 44,
                feeId: 1205,
                productName: "Vitamin D3 5000 IU",
                priceSell: 24.99m,
                onHand: 12m);

            localVitaminD = GetOrInsertDemoCatalogItem(catalogRepository, localVitaminD);

            var localVitaminDCatalogItemId = localVitaminD.CatalogItemId.Value;

            var fullscriptVitaminD = catalogMapper.FromFullscriptVariant(
                productId: "fs_prod_123",
                variantId: "fs_var_456",
                productName: "Vitamin D3 5000 IU",
                brandName: "Example Brand",
                sku: "D3-5000",
                msrp: 22.50m);

            fullscriptVitaminD = GetOrInsertDemoCatalogItem(catalogRepository, fullscriptVitaminD);

            var fullscriptVitaminDCatalogItemId = fullscriptVitaminD.CatalogItemId.Value;

            var appService = new CareItemApplicationService(
                catalogRepository,
                careItemRepository,
                pendingChargeRepository,
                fullscriptTransactionRepository);

            var localInventoryRequest = new CreateCareItemRequest
            {
                PatientId = 5001,
                PatientCaseId = 9001,
                VisitId = 30022,
                ProviderId = 17,

                CatalogItemId = localVitaminDCatalogItemId,

                SourceSystem = "EHR",
                SourceEntityType = "Supplement",
                SourceEntityId = 81233,

                CareItemType = "Supplement",

                FulfillmentSource = FulfillmentSource.LocalInventory,
                BillingIntent = BillingAction.CreatePendingCharge,
                InventoryIntent = InventoryAction.DecrementOnPost,

                QuantityRecommended = 1m,
                QuantityDispensed = 1m,

                DosageAmount = "1",
                DosageFrequency = "once per day",
                DosageDuration = "30 days",
                DosageFormat = "capsule",

                CreatedByUserId = 101
            };

            var localInventoryResponse = appService.CreateCareItem(localInventoryRequest);

            PrintCreateCareItemResponse("Create Local Inventory Care Item", localInventoryResponse);

            var fullscriptRequest = new CreateCareItemRequest
            {
                PatientId = 5001,
                PatientCaseId = 9001,
                VisitId = 30022,
                ProviderId = 17,

                CatalogItemId = fullscriptVitaminDCatalogItemId,

                SourceSystem = "EHR",
                SourceEntityType = "Supplement",
                SourceEntityId = 81234,

                CareItemType = "Supplement",

                FulfillmentSource = FulfillmentSource.Fullscript,
                BillingIntent = BillingAction.ExternalPayment,
                InventoryIntent = InventoryAction.None,

                QuantityRecommended = 1m,
                QuantityDispensed = 0m,

                DosageAmount = "1",
                DosageFrequency = "once per day",
                DosageDuration = "30 days",
                DosageFormat = "capsule",

                CreatedByUserId = 101
            };

            var fullscriptResponse = appService.CreateCareItem(fullscriptRequest);

            PrintCreateCareItemResponse("Create Fullscript Care Item", fullscriptResponse);

            Console.WriteLine("Repository counts:");
            Console.WriteLine("Catalog Items: " + catalogRepository.Search(null).Count);
            Console.WriteLine("Care Items: " + careItemRepository.GetByPatientCase(5001, 9001).Count);
            Console.WriteLine("Pending Charges: " + pendingChargeRepository.GetPendingForPatientCase(5001, 9001).Count);
            Console.WriteLine("Pending Fullscript Transactions: " + fullscriptTransactionRepository.GetPendingTransactions().Count);

            PrintPendingFullscriptTransactions(fullscriptTransactionRepository);

            FullscriptPatientMapService fullscriptPatientMapService =
                new FullscriptPatientMapService(fullscriptPatientMapRepository);

            FullscriptTransactionDispatcherService dispatcherService =
                new FullscriptTransactionDispatcherService(
                    fullscriptTransactionRepository,
                    new StubFullscriptApiClient(),
                    fullscriptPatientMapService);

            var dispatchedTransactions = dispatcherService.DispatchReadyTransactions();

            Console.WriteLine("Fullscript Transaction Dispatcher Stub");
            Console.WriteLine("Dispatched Count: " + dispatchedTransactions.Count);

            foreach (var transaction in dispatchedTransactions)
            {
                Console.WriteLine(" - FullscriptTransactionId: " + FormatNullable(transaction.FullscriptTransactionId));
                Console.WriteLine("   Status: " + Safe(transaction.Status));
                Console.WriteLine("   TreatmentPlanId: " + Safe(transaction.FullscriptTreatmentPlanId));
                Console.WriteLine("   ErrorMessage: " + Safe(transaction.ErrorMessage));
            }

            Console.WriteLine();
        }

        private static CatalogItemDto GetOrInsertDemoCatalogItem(ICatalogItemRepository catalogRepository, CatalogItemDto catalogItem)
        {
            if (catalogItem.CatalogItemId.HasValue)
            {
                return catalogItem;
            }

            var existingItems = catalogRepository.Search(catalogItem.ItemName);

            foreach (var existingItem in existingItems)
            {
                if (string.Equals(existingItem.ItemName, catalogItem.ItemName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existingItem.FullscriptVariantId ?? string.Empty, catalogItem.FullscriptVariantId ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    return existingItem;
                }
            }

            var newCatalogItemId = catalogRepository.Insert(catalogItem);
            return catalogRepository.GetById(newCatalogItemId);
        }

        private static void PrintCreateCareItemResponse(string title, CreateCareItemResponse response)
        {
            Console.WriteLine(title);
            Console.WriteLine("Success: " + response.Success);
            Console.WriteLine("Messages:");
            foreach (var message in response.Messages)
            {
                Console.WriteLine(" - " + message);
            }
            Console.WriteLine("Errors:");
            foreach (var error in response.Errors)
            {
                Console.WriteLine(" - " + error);
            }
            Console.WriteLine();
        }

        private static void PrintPendingFullscriptTransactions(IFullscriptTransactionRepository repository)
        {
            var transactions = repository.GetPendingTransactions();

            if (transactions.Count == 0)
            {
                Console.WriteLine("No ready Fullscript transactions found.");
                return;
            }

            Console.WriteLine("Ready Fullscript Transactions:");

            foreach (var transaction in transactions)
            {
                Console.WriteLine(" - FullscriptTransactionId: " + FormatNullable(transaction.FullscriptTransactionId));
                Console.WriteLine("   PatientId: " + transaction.PatientId);
                Console.WriteLine("   CareItemId: " + FormatNullable(transaction.CareItemId));
                Console.WriteLine("   CatalogItemId: " + FormatNullable(transaction.CatalogItemId));
                Console.WriteLine("   Status: " + Safe(transaction.Status));
                Console.WriteLine("   FullscriptPatientId: " + Safe(transaction.FullscriptPatientId));
                Console.WriteLine("   FullscriptPractitionerId: " + Safe(transaction.FullscriptPractitionerId));
                Console.WriteLine("   FullscriptProductId: " + Safe(transaction.FullscriptProductId));
                Console.WriteLine("   FullscriptVariantId: " + Safe(transaction.FullscriptVariantId));
            }

            Console.WriteLine();
        }

        private static string FormatNullable(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "";
        }

        private static string Safe(string value)
        {
            return value ?? "";
        }

        private static async Task RunFullscriptAuthorizeUrlDemo()
        {
            Console.WriteLine("Fullscript OAuth/API demo");

            var config = FullscriptConfiguration.LoadFromEnvironment();

            if (!config.IsComplete())
            {
                Console.WriteLine("Missing Fullscript OAuth/API environment variables.");
                Console.WriteLine("Set these before running the demo:");
                Console.WriteLine(" - FULLSCRIPT_CLIENT_ID");
                Console.WriteLine(" - FULLSCRIPT_CLIENT_SECRET");
                Console.WriteLine(" - FULLSCRIPT_REDIRECT_URI");
                Console.WriteLine(" - FULLSCRIPT_PRACTITIONER_ID");
                Console.WriteLine();
                return;
            }

            var oauthClient = new FullscriptOAuthClient(config);

            var authorizeUrl = oauthClient.BuildAuthorizeUrl("sample-state-123");

            Console.WriteLine("Authorize URL:");
            Console.WriteLine(authorizeUrl);

            Console.WriteLine();
            Console.WriteLine("Open this URL in a browser, approve access, then copy the code query-string value.");
            Console.Write("Authorization code: ");
            var authorizationCode = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(authorizationCode))
            {
                Console.WriteLine("No authorization code entered. Skipping token exchange.");
                return;
            }

            var tokenResponse = await oauthClient.ExchangeCodeForTokenAsync(authorizationCode);

            Console.WriteLine();
            Console.WriteLine("Access Token: " + tokenResponse.AccessToken);
            Console.WriteLine("Refresh Token: " + tokenResponse.RefreshToken);
            Console.WriteLine("Expires In: " + tokenResponse.ExpiresIn);

            var apiClient = new FullscriptApiClient(config);
            var productsJson = await apiClient.GetProductsJsonAsync(tokenResponse.AccessToken);

            Console.WriteLine();
            Console.WriteLine("Products response:");
            Console.WriteLine(productsJson);
        }
    }
}