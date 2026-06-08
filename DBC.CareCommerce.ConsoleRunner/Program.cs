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

            var dispatcherService = new FullscriptTransactionDispatcherService(fullscriptTransactionRepository);

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

            PrintPendingFullscriptTransactions(fullscriptTransactionRepository);
        }

        private static void PrintPendingFullscriptTransactions(IFullscriptTransactionRepository fullscriptTransactionRepository)
        {
            if (fullscriptTransactionRepository == null)
            {
                throw new ArgumentNullException("fullscriptTransactionRepository");
            }

            var pendingTransactions = fullscriptTransactionRepository.GetPendingTransactions();

            Console.WriteLine();
            Console.WriteLine("Pending Fullscript Transaction Smoke Test");
            Console.WriteLine("Count: " + pendingTransactions.Count);

            foreach (var transaction in pendingTransactions)
            {
                Console.WriteLine(" - FullscriptTransactionId: " + FormatNullable(transaction.FullscriptTransactionId));
                Console.WriteLine("   CareItemId: " + FormatNullable(transaction.CareItemId));
                Console.WriteLine("   CatalogItemId: " + FormatNullable(transaction.CatalogItemId));
                Console.WriteLine("   PatientId: " + transaction.PatientId);
                Console.WriteLine("   PatientCaseId: " + FormatNullable(transaction.PatientCaseId));
                Console.WriteLine("   ProviderId: " + FormatNullable(transaction.ProviderId));
                Console.WriteLine("   ProductId: " + Safe(transaction.FullscriptProductId));
                Console.WriteLine("   VariantId: " + Safe(transaction.FullscriptVariantId));
                Console.WriteLine("   TreatmentPlanId: " + Safe(transaction.FullscriptTreatmentPlanId));
                Console.WriteLine("   OrderId: " + Safe(transaction.FullscriptOrderId));
                Console.WriteLine("   Status: " + Safe(transaction.Status));
            }

            Console.WriteLine();
        }

        private static void PrintCreateCareItemResponse(string title, CreateCareItemResponse response)
        {
            Console.WriteLine("======================================");
            Console.WriteLine(title);
            Console.WriteLine("Success: " + response.Success);
            Console.WriteLine("CatalogItemId: " + FormatNullable(response.CatalogItemId));
            Console.WriteLine("CareItemId: " + FormatNullable(response.CareItemId));
            Console.WriteLine("PendingChargeId: " + FormatNullable(response.PendingChargeId));
            Console.WriteLine("FullscriptTransactionId: " + FormatNullable(response.FullscriptTransactionId));

            if (response.WorkflowDecision != null)
            {
                Console.WriteLine("Workflow:");
                Console.WriteLine(" - Fulfillment: " + response.WorkflowDecision.FulfillmentSource);
                Console.WriteLine(" - Billing: " + response.WorkflowDecision.BillingAction);
                Console.WriteLine(" - Inventory: " + response.WorkflowDecision.InventoryAction);
                Console.WriteLine(" - Create Pending Charge: " + response.WorkflowDecision.ShouldCreatePendingCharge);
                Console.WriteLine(" - Create Fullscript Transaction: " + response.WorkflowDecision.ShouldCreateFullscriptTransaction);
                Console.WriteLine(" - Affect Local Inventory: " + response.WorkflowDecision.ShouldAffectLocalInventory);
            }

            if (response.PendingCharge != null)
            {
                Console.WriteLine("Pending Charge:");
                Console.WriteLine(" - Description: " + response.PendingCharge.Description);
                Console.WriteLine(" - Quantity: " + response.PendingCharge.Quantity);
                Console.WriteLine(" - Unit Amount: " + response.PendingCharge.UnitAmount);
                Console.WriteLine(" - Total Amount: " + response.PendingCharge.TotalAmount);
                Console.WriteLine(" - FeeId: " + FormatNullable(response.PendingCharge.FeeId));
                Console.WriteLine(" - ProductId: " + FormatNullable(response.PendingCharge.ProductId));
            }

            if (response.FullscriptTransaction != null)
            {
                Console.WriteLine("Fullscript Transaction Decision:");
                Console.WriteLine(" - Status: " + response.FullscriptTransaction.Status);
                Console.WriteLine(" - ProductId: " + Safe(response.FullscriptTransaction.FullscriptProductId));
                Console.WriteLine(" - VariantId: " + Safe(response.FullscriptTransaction.FullscriptVariantId));
                Console.WriteLine(" - PatientId: " + response.FullscriptTransaction.PatientId);
                Console.WriteLine(" - PatientCaseId: " + FormatNullable(response.FullscriptTransaction.PatientCaseId));
                Console.WriteLine(" - ProviderId: " + FormatNullable(response.FullscriptTransaction.ProviderId));
            }

            if (response.Messages.Count > 0)
            {
                Console.WriteLine("Messages:");
                foreach (var message in response.Messages)
                {
                    Console.WriteLine(" - " + message);
                }
            }

            if (response.Warnings.Count > 0)
            {
                Console.WriteLine("Warnings:");
                foreach (var warning in response.Warnings)
                {
                    Console.WriteLine(" - " + warning);
                }
            }

            if (response.Errors.Count > 0)
            {
                Console.WriteLine("Errors:");
                foreach (var error in response.Errors)
                {
                    Console.WriteLine(" - " + error);
                }
            }

            Console.WriteLine();
        }

        private static string FormatNullable(int? value)
        {
            return value.HasValue ? value.Value.ToString() : "(none)";
        }

        private static string Safe(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(none)" : value;
        }

        private static async Task RunFullscriptAuthorizeUrlDemo()
        {
            Console.WriteLine("======================================");
            Console.WriteLine("Fullscript OAuth Authorize URL Demo");
            Console.WriteLine();

            //var fullscriptPatientMapRepository = new InMemoryFullscriptPatientMapRepository();
            //var fullscriptConnectionRepository = new InMemoryFullscriptConnectionRepository();

            var connectionString = Environment.GetEnvironmentVariable("DBC_CARECOMMERCE_SQL_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Console.WriteLine("Missing SQL connection string.");
                Console.WriteLine("Set environment variable DBC_CARECOMMERCE_SQL_CONNECTION before running the console demo.");
                return;
            }

            var sqlConnectionFactory = new SqlConnectionFactory(connectionString);

            IFullscriptConnectionRepository fullscriptConnectionRepository = new SqlFullscriptConnectionRepository(sqlConnectionFactory);
            IFullscriptPatientMapRepository fullscriptPatientMapRepository = new SqlFullscriptPatientMapRepository(sqlConnectionFactory);

            var tokenEncryptionService = new CompositeTokenEncryptionService(new DpapiTokenEncryptionService(), new DevelopmentTokenEncryptionService());

            Console.Write("Enter Fullscript Client ID: ");
            var clientId = Console.ReadLine();

            Console.Write("Enter scope, or press ENTER to leave blank: ");
            var scope = Console.ReadLine();

            var configuration = new FullscriptConfiguration
            {
                Environment = FullscriptEnvironment.UsSandbox,
                ClientId = clientId,
                RedirectUri = "http://localhost:5000/fullscript/oauth/callback",
                Scope = string.IsNullOrWhiteSpace(scope) ? null : scope
            };

            var state = Guid.NewGuid().ToString("N");

            var builder = new FullscriptAuthorizeUrlBuilder();
            var authorizeUrl = builder.BuildAuthorizeUrl(configuration, state);

            Console.WriteLine();
            Console.WriteLine("Starting local callback listener...");
            Console.WriteLine("Listening at: " + configuration.RedirectUri);
            Console.WriteLine();

            var listener = new FullscriptLocalCallbackListener("http://localhost:5000/fullscript/oauth/callback/");

            var callbackTask = listener.WaitForCallbackAsync(TimeSpan.FromMinutes(2));

            Console.WriteLine("Opening browser...");
            OpenUrl(authorizeUrl);

            Console.WriteLine("Waiting for Fullscript callback...");
            Console.WriteLine();

            var callbackResult = await callbackTask;

            if (callbackResult.TimedOut)
            {
                Console.WriteLine("No callback received before timeout.");
                Console.WriteLine("Fullscript did not redirect back to localhost during this run.");
                Console.WriteLine("This is expected while the app/account is still in pre-authorization.");
                return;
            }

            if (callbackResult.HasError())
            {
                Console.WriteLine("Fullscript returned an error:");
                Console.WriteLine(callbackResult.Error);
                Console.WriteLine(callbackResult.ErrorDescription);
                return;
            }

            if (!callbackResult.HasCode())
            {
                Console.WriteLine("Callback received, but no authorization code was found.");
                return;
            }

            Console.WriteLine("Authorization code received:");
            Console.WriteLine(callbackResult.Code);
            Console.WriteLine();

            Console.WriteLine("State received:");
            Console.WriteLine(callbackResult.State);
            Console.WriteLine();

            if (string.IsNullOrWhiteSpace(callbackResult.State))
            {
                Console.WriteLine("WARNING: Fullscript did not return a state value in the callback.");
            }
            else if (!string.Equals(callbackResult.State, state, StringComparison.Ordinal))
            {
                Console.WriteLine("WARNING: Returned state does not match original state.");
            }

            Console.Write("Enter Fullscript Client Secret to exchange token, or press ENTER to skip: ");
            var clientSecret = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                Console.WriteLine("Token exchange skipped.");
                return;
            }

            configuration.ClientSecret = clientSecret;

            Console.WriteLine("Client secret received. Preparing OAuth application service...");

            var oauthService = new FullscriptOAuthApplicationService(fullscriptConnectionRepository, tokenEncryptionService);

            FullscriptConnectionDto savedConnection;

            try
            {
                Console.WriteLine();
                Console.WriteLine("Completing Fullscript authorization through FullscriptOAuthApplicationService...");

                savedConnection = await oauthService.CompleteAuthorizationAsync(
                    configuration,
                    callbackResult.Code);

                Console.WriteLine("Fullscript authorization completed and connection saved.");
                Console.WriteLine(" - FullscriptConnectionId: " + FormatNullable(savedConnection.FullscriptConnectionId));
                Console.WriteLine(" - Environment: " + Safe(savedConnection.Environment));
                Console.WriteLine(" - Clinic ID: " + Safe(savedConnection.ClinicId));
                Console.WriteLine(" - Clinic Name: " + Safe(savedConnection.ClinicName));
                Console.WriteLine(" - Scope: " + Safe(savedConnection.Scope));
                Console.WriteLine(" - Token Type: " + Safe(savedConnection.TokenType));
                Console.WriteLine(" - Token Expires At UTC: " + (savedConnection.TokenExpiresAtDateTime.HasValue ? savedConnection.TokenExpiresAtDateTime.Value.ToString("u") : "(none)"));
                Console.WriteLine(" - Has Access Token Stored: " + savedConnection.HasAccessToken());
                Console.WriteLine(" - Has Refresh Token Stored: " + savedConnection.HasRefreshToken());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fullscript authorization completion failed.");
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine();
            Console.Write("Refresh access token now using saved refresh token? Y/N: ");
            var refreshAnswer = Console.ReadLine();

            if (string.Equals(refreshAnswer, "Y", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Console.WriteLine();
                    Console.WriteLine("Refreshing Fullscript connection through FullscriptOAuthApplicationService...");

                    var refreshedConnection = await oauthService.RefreshConnectionAsync(
                        configuration,
                        savedConnection);

                    savedConnection = refreshedConnection;

                    Console.WriteLine("Fullscript connection refreshed.");
                    Console.WriteLine(" - FullscriptConnectionId: " + FormatNullable(savedConnection.FullscriptConnectionId));
                    Console.WriteLine(" - Token Expires At UTC: " + (savedConnection.TokenExpiresAtDateTime.HasValue ? savedConnection.TokenExpiresAtDateTime.Value.ToString("u") : "(none)"));
                    Console.WriteLine(" - Last Refresh UTC: " + (savedConnection.LastRefreshDateTime.HasValue ? savedConnection.LastRefreshDateTime.Value.ToString("u") : "(none)"));
                    Console.WriteLine(" - Has Access Token Stored: " + savedConnection.HasAccessToken());
                    Console.WriteLine(" - Has Refresh Token Stored: " + savedConnection.HasRefreshToken());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Token refresh failed.");
                    Console.WriteLine(ex.Message);
                }
            }

            if (!savedConnection.HasAccessToken())
            {
                Console.WriteLine("Saved Fullscript connection does not contain an access token. API calls skipped.");
                return;
            }

            var apiClient = new FullscriptApiClient(configuration);
            var accessToken = savedConnection.AccessTokenEncrypted;

            /*
             * Patient List
             */

            Console.WriteLine();
            Console.WriteLine("Calling Fullscript GET /api/clinic/patients...");

            try
            {
                var patientsJson = await apiClient.GetPatientsRawJsonAsync(accessToken);

                Console.WriteLine("GET /api/clinic/patients succeeded.");
                Console.WriteLine("Raw patients JSON:");
                Console.WriteLine(patientsJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine("GET /api/clinic/patients failed.");
                Console.WriteLine(ex.Message);
            }

            /*
             * Patient Create / Map
             */

            Console.WriteLine();
            Console.Write("Create or map a sandbox Fullscript patient using FullscriptPatientService? Y/N: ");
            var createPatientAnswer = Console.ReadLine();

            if (string.Equals(createPatientAnswer, "Y", StringComparison.OrdinalIgnoreCase))
            {
                var uniqueSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

                var dbcPatientId = 5001;
                var firstName = "DBC";
                var lastName = "Sandbox " + uniqueSuffix;
                var email = "dbc.fullscript.sandbox+" + uniqueSuffix + "@example.com";
                var environment = savedConnection.Environment;
                var clinicId = savedConnection.ClinicId;

                Console.WriteLine();
                Console.WriteLine("Calling FullscriptPatientService.GetOrCreatePatientMapAsync...");
                Console.WriteLine("Creating fake sandbox patient if no existing map is found:");
                Console.WriteLine(" - DBC PatientId: " + dbcPatientId);
                Console.WriteLine(" - First Name: " + firstName);
                Console.WriteLine(" - Last Name: " + lastName);
                Console.WriteLine(" - Email: " + email);
                Console.WriteLine(" - Environment: " + Safe(environment));
                Console.WriteLine(" - ClinicId: " + Safe(clinicId));

                try
                {
                    var patientService = new FullscriptPatientService(
                        apiClient,
                        fullscriptPatientMapRepository);

                    var map = await patientService.GetOrCreatePatientMapAsync(
                        accessToken,
                        dbcPatientId,
                        firstName,
                        lastName,
                        email,
                        environment,
                        clinicId);

                    Console.WriteLine("Fullscript patient map returned.");
                    Console.WriteLine(" - FullscriptPatientMapId: " + FormatNullable(map.FullscriptPatientMapId));
                    Console.WriteLine(" - DBC PatientId: " + map.PatientId);
                    Console.WriteLine(" - Fullscript PatientId: " + Safe(map.FullscriptPatientId));
                    Console.WriteLine(" - Metadata ID: " + Safe(map.FullscriptMetadataId));
                    Console.WriteLine(" - Email: " + Safe(map.FullscriptEmail));
                    Console.WriteLine(" - First Name: " + Safe(map.FullscriptFirstName));
                    Console.WriteLine(" - Last Name: " + Safe(map.FullscriptLastName));

                    var existingMap = patientService.GetExistingPatientMap(
                        dbcPatientId,
                        environment,
                        clinicId);

                    Console.WriteLine();
                    Console.WriteLine("Existing map lookup after create:");
                    Console.WriteLine(" - Found: " + (existingMap != null));

                    if (existingMap != null)
                    {
                        Console.WriteLine(" - Fullscript PatientId: " + Safe(existingMap.FullscriptPatientId));
                        Console.WriteLine(" - Metadata ID: " + Safe(existingMap.FullscriptMetadataId));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("FullscriptPatientService test failed.");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void OpenUrl(string url)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        private static CatalogItemDto GetOrInsertDemoCatalogItem(ICatalogItemRepository catalogRepository, CatalogItemDto item)
        {
            if (catalogRepository == null)
            {
                throw new ArgumentNullException("catalogRepository");
            }

            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            CatalogItemDto existing = null;

            if (item.ProductId.HasValue)
            {
                existing = catalogRepository.GetByProductId(item.ProductId.Value);
            }

            if (existing == null && item.SupplementId.HasValue)
            {
                existing = catalogRepository.GetBySupplementId(item.SupplementId.Value);
            }

            if (existing == null && !string.IsNullOrWhiteSpace(item.FullscriptVariantId))
            {
                SqlCatalogItemRepository sqlCatalogRepository = catalogRepository as SqlCatalogItemRepository;

                if (sqlCatalogRepository != null)
                {
                    existing = sqlCatalogRepository.GetByFullscriptVariantId(item.FullscriptVariantId);
                }
            }

            if (existing != null)
            {
                return existing;
            }

            int newId = catalogRepository.Insert(item);
            return catalogRepository.GetById(newId);
        }
    }
}