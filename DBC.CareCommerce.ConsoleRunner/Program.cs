using System;
using System.Diagnostics;
using DBC.Integrations.Fullscript.Configuration;
using DBC.Integrations.Fullscript.OAuth;
using DBC.Integrations.Fullscript.Client;
using DBC.Integrations.Fullscript.Models;
using DBC.Integrations.Fullscript.Services;
using DBC.CareCommerce.Application.Services;
using DBC.CareCommerce.Contracts.Enums;
using DBC.CareCommerce.Contracts.Mapping;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Requests;
using DBC.CareCommerce.Contracts.Responses;
using DBC.CareCommerce.Data.InMemory;



namespace DBC.CareCommerce.ConsoleRunner
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                await RunFullscriptAuthorizeUrlDemo();

                RunApplicationServiceDemo();

                Console.WriteLine();
                Console.WriteLine("Care Commerce application-service demo completed.");
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
            var catalogRepository = new InMemoryCatalogItemRepository();
            var careItemRepository = new InMemoryCareItemRepository();
            var pendingChargeRepository = new InMemoryPendingChargeRepository();

            var catalogMapper = new CatalogItemMapper();

            var localVitaminD = catalogMapper.FromProduct(
                productId: 44,
                feeId: 1205,
                productName: "Vitamin D3 5000 IU",
                priceSell: 24.99m,
                onHand: 12m);

            var localVitaminDCatalogItemId = catalogRepository.Insert(localVitaminD);

            var fullscriptVitaminD = catalogMapper.FromFullscriptVariant(
                productId: "fs_prod_123",
                variantId: "fs_var_456",
                productName: "Vitamin D3 5000 IU",
                brandName: "Example Brand",
                sku: "D3-5000",
                msrp: 22.50m);

            var fullscriptVitaminDCatalogItemId = catalogRepository.Insert(fullscriptVitaminD);

            var appService = new CareItemApplicationService(
                catalogRepository,
                careItemRepository,
                pendingChargeRepository);

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
            Console.WriteLine("Catalog Items: " + catalogRepository.GetAll().Count);
            Console.WriteLine("Care Items: " + careItemRepository.GetAll().Count);
            Console.WriteLine("Pending Charges: " + pendingChargeRepository.GetAll().Count);
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


            var fullscriptPatientMapRepository = new InMemoryFullscriptPatientMapRepository();



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

            if (!string.Equals(callbackResult.State, state, StringComparison.Ordinal))
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

            Console.WriteLine("Client secret received. Preparing token exchange...");

            var tokenClient = new FullscriptTokenClient();

            Console.WriteLine();
            Console.WriteLine("Exchanging authorization code for token...");

            var token = await tokenClient.ExchangeAuthorizationCodeAsync(configuration, callbackResult.Code);

            Console.WriteLine("Token exchange succeeded.");
            Console.WriteLine("Token Type: " + Safe(token.TokenType));
            Console.WriteLine("Has Access Token: " + token.HasAccessToken());
            Console.WriteLine("Expires In: " + (token.ExpiresIn.HasValue ? token.ExpiresIn.Value.ToString() : "(none)"));
            Console.WriteLine("Expires At UTC: " + (token.ExpiresAtUtc.HasValue ? token.ExpiresAtUtc.Value.ToString("u") : "(none)"));
            Console.WriteLine("Scope: " + Safe(token.Scope));
            Console.WriteLine("Has Refresh Token: " + (!string.IsNullOrWhiteSpace(token.RefreshToken)));

            if (token.HasAccessToken())
            {
                var apiClient = new FullscriptApiClient(configuration);

                /*
                 * Clinic List
                 */

                Console.WriteLine();
                Console.WriteLine("Calling Fullscript GET /api/clinic...");

                try
                {
                    var clinicJson = await apiClient.GetClinicRawJsonAsync(token.AccessToken);

                    Console.WriteLine("GET /api/clinic succeeded.");
                    Console.WriteLine("Raw clinic JSON:");
                    Console.WriteLine(clinicJson);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET /api/clinic failed.");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Continuing because catalog/product search may still work with catalog:read.");
                }

                /*
                 * Patient List
                 */

                Console.WriteLine();
                Console.WriteLine("Calling Fullscript GET /api/clinic/patients...");

                try
                {
                    var patientsJson = await apiClient.GetPatientsRawJsonAsync(token.AccessToken);

                    Console.WriteLine("GET /api/clinic/patients succeeded.");
                    Console.WriteLine("Raw patients JSON:");
                    Console.WriteLine(patientsJson);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GET /api/clinic/patients failed.");
                    Console.WriteLine(ex.Message);
                }



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
                    var environment = "UsSandbox";
                    string clinicId = null;

                    Console.WriteLine();
                    Console.WriteLine("Calling FullscriptPatientService.GetOrCreatePatientMapAsync...");
                    Console.WriteLine("Creating fake sandbox patient if no existing map is found:");
                    Console.WriteLine(" - DBC PatientId: " + dbcPatientId);
                    Console.WriteLine(" - First Name: " + firstName);
                    Console.WriteLine(" - Last Name: " + lastName);
                    Console.WriteLine(" - Email: " + email);
                    Console.WriteLine(" - Environment: " + environment);
                    Console.WriteLine(" - ClinicId: " + Safe(clinicId));

                    try
                    {
                        var patientService = new FullscriptPatientService(
                            apiClient,
                            fullscriptPatientMapRepository);

                        var map = await patientService.GetOrCreatePatientMapAsync(
                            token.AccessToken,
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




                //Console.WriteLine();
                //Console.Write("Create a sandbox Fullscript patient? Y/N: ");
                //var createPatientAnswer = Console.ReadLine();

                //if (string.Equals(createPatientAnswer, "Y", StringComparison.OrdinalIgnoreCase))
                //{
                //    var uniqueSuffix = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

                //    var firstName = "DBC";
                //    var lastName = "Sandbox " + uniqueSuffix;
                //    var email = "dbc.fullscript.sandbox+" + uniqueSuffix + "@example.com";

                //    Console.WriteLine();
                //    Console.WriteLine("Calling Fullscript POST /api/clinic/patients...");
                //    Console.WriteLine("Creating fake sandbox patient:");
                //    Console.WriteLine(" - First Name: " + firstName);
                //    Console.WriteLine(" - Last Name: " + lastName);
                //    Console.WriteLine(" - Email: " + email);

                //    try
                //    {
                //        var metadataId = "DBC-PATIENT-5001";

                //        var createPatientJson = await apiClient.CreatePatientRawJsonAsync(
                //            token.AccessToken,
                //            firstName,
                //            lastName,
                //            email,
                //            metadataId);

                //        Console.WriteLine("Create patient succeeded.");
                //        Console.WriteLine("Raw create patient JSON:");
                //        Console.WriteLine(createPatientJson);

                //        var parsedPatient = FullscriptPatientResponseParser.ParseSinglePatientResponse(createPatientJson);

                //        Console.WriteLine("Parsed Fullscript Patient:");
                //        Console.WriteLine(" - Patient ID: " + Safe(parsedPatient.PatientId));
                //        Console.WriteLine(" - First Name: " + Safe(parsedPatient.FirstName));
                //        Console.WriteLine(" - Last Name: " + Safe(parsedPatient.LastName));
                //        Console.WriteLine(" - Email: " + Safe(parsedPatient.Email));
                //        Console.WriteLine(" - Metadata ID: " + Safe(parsedPatient.MetadataId));

                //        if (parsedPatient.HasPatientId())
                //        {
                //            var map = new FullscriptPatientMapDto
                //            {
                //                PatientId = 5001,
                //                FullscriptPatientId = parsedPatient.PatientId,
                //                FullscriptMetadataId = parsedPatient.MetadataId,
                //                FullscriptEmail = parsedPatient.Email,
                //                FullscriptFirstName = parsedPatient.FirstName,
                //                FullscriptLastName = parsedPatient.LastName,
                //                Environment = "UsSandbox",
                //                ClinicId = null,
                //                LastSyncedDateTime = DateTime.UtcNow
                //            };

                //            var mapId = fullscriptPatientMapRepository.Insert(map);

                //            Console.WriteLine("Fullscript patient map saved.");
                //            Console.WriteLine(" - FullscriptPatientMapId: " + mapId);
                //            Console.WriteLine(" - DBC PatientId: " + map.PatientId);
                //            Console.WriteLine(" - Fullscript PatientId: " + map.FullscriptPatientId);
                //            Console.WriteLine(" - Metadata ID: " + Safe(map.FullscriptMetadataId));
                //        }
                //        else
                //        {
                //            Console.WriteLine("Fullscript patient response did not include a patient ID. Map was not saved.");
                //        }




                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine("Create patient failed.");
                //        Console.WriteLine(ex.Message);
                //    }
                //}


                /* 
                 * Patient Search
                 */


                //Console.WriteLine();
                //Console.Write("Enter Fullscript patient email for field search, or press ENTER to skip: ");
                //var patientEmail = Console.ReadLine();

                //if (!string.IsNullOrWhiteSpace(patientEmail))
                //{
                //    Console.WriteLine();
                //    Console.WriteLine("Calling Fullscript patient field search by email...");

                //    try
                //    {
                //        var fieldSearchJson = await apiClient.SearchPatientsByFieldsRawJsonAsync(
                //            token.AccessToken,
                //            null,
                //            null,
                //            patientEmail);

                //        Console.WriteLine("Patient field search succeeded.");
                //        Console.WriteLine("Raw patient field search JSON:");
                //        Console.WriteLine(fieldSearchJson);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine("Patient field search failed.");
                //        Console.WriteLine(ex.Message);
                //    }
                //}

                /*
                 * Product search
                 */

                //Console.WriteLine();
                //Console.Write("Enter a Fullscript product search term, or press ENTER to skip: ");
                //var productSearch = Console.ReadLine();

                //if (!string.IsNullOrWhiteSpace(productSearch))
                //{
                //    Console.WriteLine();
                //    Console.WriteLine("Calling Fullscript product search...");

                //    var productJson = await apiClient.SearchProductsRawJsonAsync(token.AccessToken, productSearch);

                //    Console.WriteLine("Product search succeeded.");
                //    Console.WriteLine("Raw product JSON:");
                //    Console.WriteLine(productJson);
                //}

                /*
                 * Full Product search
                 */

                //Console.WriteLine();
                //Console.WriteLine("Calling Fullscript GET /api/catalog/products...");

                //try
                //{
                //    var productsJson = await apiClient.GetProductsRawJsonAsync(token.AccessToken);

                //    Console.WriteLine("GET /api/catalog/products succeeded.");
                //    Console.WriteLine("Raw products JSON:");
                //    Console.WriteLine(productsJson);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine("GET /api/catalog/products failed.");
                //    Console.WriteLine(ex.Message);
                //}


            }
            else
            {
                Console.WriteLine("Token exchange did not return an access token. API calls skipped.");
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
    }
}