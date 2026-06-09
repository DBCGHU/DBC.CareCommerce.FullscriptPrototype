using System;
using System.Collections.Generic;
using DBC.CareCommerce.Application.Services;
using DBC.CareCommerce.Contracts.Models;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Contracts.Requests;
using DBC.CareCommerce.Contracts.Services;
using DBC.CareCommerce.Contracts.Services.Contracts;
using DBC.CareCommerce.Data.DataAccess;
using DBC.CareCommerce.Data.Repositories;
using DBC.CareCommerce.WindowsService.Services;
using DBC.Integrations.Fullscript.Services;
using DBC.Integrations.Fullscript.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DBC.CareCommerce.WindowsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplication app = CreateApplication(args);

            app.MapGet(
                "/health",
                () =>
                {
                    return Results.Ok(new
                    {
                        status = "Healthy",
                        service = "DBC Care Commerce Windows Service",
                        timestampUtc = DateTime.UtcNow
                    });
                });

            app.MapGet(
                "/ready",
                () =>
                {
                    bool sqlConnectionConfigured =
                        !string.IsNullOrWhiteSpace(
                            Environment.GetEnvironmentVariable("DBC_CARECOMMERCE_SQL_CONNECTION"));

                    bool localTokenConfigured =
                        !string.IsNullOrWhiteSpace(
                            Environment.GetEnvironmentVariable("DBC_CARECOMMERCE_LOCAL_TOKEN"));

                    bool ready =
                        sqlConnectionConfigured &&
                        localTokenConfigured;

                    return Results.Ok(new
                    {
                        status = ready ? "Ready" : "NotReady",
                        sqlConnectionConfigured = sqlConnectionConfigured,
                        localTokenConfigured = localTokenConfigured,
                        backgroundWorkerEnabled = true,
                        timestampUtc = DateTime.UtcNow
                    });
                });

            app.MapGet(
                "/fullscript/oauth/start",
                (FullscriptOAuthDiagnosticService oauthDiagnosticService) =>
                {
                    string authorizeUrl = oauthDiagnosticService.BuildAuthorizeUrl();

                    if (string.IsNullOrWhiteSpace(authorizeUrl))
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            errorMessage = "Fullscript OAuth authorize URL is not configured."
                        });
                    }

                    return Results.Redirect(authorizeUrl);
                });

            app.MapGet(
                "/fullscript/oauth/callback",
                (HttpRequest httpRequest, FullscriptOAuthDiagnosticService oauthDiagnosticService) =>
                {
                    string error = httpRequest.Query["error"].ToString();

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            error = error,
                            errorDescription = httpRequest.Query["error_description"].ToString()
                        });
                    }

                    string code = httpRequest.Query["code"].ToString();

                    return Results.Ok(oauthDiagnosticService.ExchangeCodeForToken(code));
                });

            app.MapGet(
                "/fullscript/oauth/token-status",
                (
                    HttpRequest httpRequest,
                    FullscriptOAuthDiagnosticService oauthDiagnosticService,
                    LocalMiddlewareAuthorizationService authorizationService) =>
                {
                    if (!authorizationService.IsAuthorized(
                        httpRequest.Headers[LocalMiddlewareAuthorizationService.HeaderName].ToString()))
                    {
                        return Results.Unauthorized();
                    }

                    return Results.Ok(oauthDiagnosticService.GetCurrentTokenDiagnostic());
                });

            app.MapPost(
                "/care-commerce/recommendations/validate",
                (
                    HttpRequest httpRequest,
                    SubmitCareRecommendationRequest request,
                    SubmitCareRecommendationRequestValidator validator,
                    LocalMiddlewareAuthorizationService authorizationService) =>
                {
                    if (!authorizationService.IsAuthorized(
                        httpRequest.Headers[LocalMiddlewareAuthorizationService.HeaderName].ToString()))
                    {
                        return Results.Unauthorized();
                    }

                    List<string> errors = validator.Validate(request);

                    return Results.Ok(new
                    {
                        success = errors.Count == 0,
                        errors = errors,
                        warnings = new List<string>(),
                        messages = new List<string>
                        {
                            "Validation completed. No records were created."
                        }
                    });
                });

            app.MapGet(
                "/care-commerce/recommendations/{careItemId:int}",
                (int careItemId, CareCommerceRecommendationReadService readService) =>
                {
                    return Results.Ok(readService.GetByCareItemId(careItemId));
                });

            app.MapGet(
                "/fullscript/transactions/ready",
                (FullscriptTransactionReadService readService) =>
                {
                    return Results.Ok(readService.GetReadyTransactions());
                });

            app.MapGet(
                "/fullscript/transactions/{fullscriptTransactionId:int}",
                (int fullscriptTransactionId, FullscriptTransactionReadService readService) =>
                {
                    return Results.Ok(readService.GetByFullscriptTransactionId(fullscriptTransactionId));
                });

            app.MapPost(
                "/fullscript/patients/{patientId:int}/sync-test",
                (
                    HttpRequest httpRequest,
                    int patientId,
                    FullscriptPatientSyncService patientSyncService,
                    LocalMiddlewareAuthorizationService authorizationService) =>
                {
                    if (!authorizationService.IsAuthorized(
                        httpRequest.Headers[LocalMiddlewareAuthorizationService.HeaderName].ToString()))
                    {
                        return Results.Unauthorized();
                    }

                    FullscriptPatientCreateResultDto result =
                        patientSyncService.CreatePatientForLocalPatient(patientId);

                    return Results.Ok(new
                    {
                        success = result.Success,
                        patientId = patientId,
                        fullscriptPatientId = result.FullscriptPatientId,
                        errorMessage = result.ErrorMessage,
                        message = "Diagnostic patient sync completed. No treatment plan was dispatched."
                    });
                });

            app.MapPost(
                "/fullscript/dispatch",
                (
                    HttpRequest httpRequest,
                    FullscriptTransactionDispatcherService dispatcherService,
                    LocalMiddlewareAuthorizationService authorizationService) =>
                {
                    if (!authorizationService.IsAuthorized(
                        httpRequest.Headers[LocalMiddlewareAuthorizationService.HeaderName].ToString()))
                    {
                        return Results.Unauthorized();
                    }

                    return Results.Ok(dispatcherService.DispatchReadyTransactions());
                });

            app.MapPost(
                "/care-commerce/recommendations",
                (
                    HttpRequest httpRequest,
                    SubmitCareRecommendationRequest request,
                    CareCommerceMiddlewareCommandService commandService,
                    LocalMiddlewareAuthorizationService authorizationService) =>
                {
                    if (!authorizationService.IsAuthorized(
                        httpRequest.Headers[LocalMiddlewareAuthorizationService.HeaderName].ToString()))
                    {
                        return Results.Unauthorized();
                    }

                    return Results.Ok(commandService.SubmitCareRecommendation(request));
                });
            app.Run();
        }

        public static WebApplication CreateApplication(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Host.UseWindowsService();

            builder.WebHost.UseUrls("http://127.0.0.1:5147");

            builder.Services.Configure<CareCommerceServiceSettings>(
                builder.Configuration.GetSection("CareCommerceService"));

            builder.Services.Configure<FullscriptApiSettings>(
                builder.Configuration.GetSection("FullscriptApi"));

            builder.Services.AddSingleton(provider =>
            {
                CareCommerceServiceSettings settings =
                    builder.Configuration
                        .GetSection("CareCommerceService")
                        .Get<CareCommerceServiceSettings>() ?? new CareCommerceServiceSettings();

                string connectionString =
                    Environment.GetEnvironmentVariable("DBC_CARECOMMERCE_SQL_CONNECTION") ?? string.Empty;

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString = settings.SqlConnectionString ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString =
                        builder.Configuration["CareCommerceService:SqlConnectionString"] ?? string.Empty;
                }

                return new SqlConnectionFactory(connectionString);
            });

            builder.Services.AddSingleton<IFullscriptOAuthTokenProvider, InMemoryFullscriptOAuthTokenProvider>();

            builder.Services.AddScoped<ICatalogItemRepository, SqlCatalogItemRepository>();
            builder.Services.AddScoped<ICareItemRepository, SqlCareItemRepository>();
            builder.Services.AddScoped<IPendingChargeRepository, SqlPendingChargeRepository>();
            builder.Services.AddScoped<IFullscriptTransactionRepository, SqlFullscriptTransactionRepository>();
            builder.Services.AddScoped<IPatientProfileRepository, SqlPatientProfileRepository>();
            builder.Services.AddScoped<IFullscriptPatientMapRepository, SqlFullscriptPatientMapRepository>();

            builder.Services.AddScoped<ICareItemApplicationService, CareItemApplicationService>();
            builder.Services.AddScoped<ICareCommerceIntegrationService, CareCommerceIntegrationService>();

            builder.Services.AddScoped<CareCommerceMiddlewareCommandService>();
            builder.Services.AddScoped<CareCommerceRecommendationReadService>();
            builder.Services.AddScoped<FullscriptTransactionReadService>();
            builder.Services.AddScoped<FullscriptPatientMapService>();
            builder.Services.AddScoped<FullscriptPatientSyncService>();
            builder.Services.AddScoped<LocalMiddlewareAuthorizationService>();
            builder.Services.AddScoped<SubmitCareRecommendationRequestValidator>();
            builder.Services.AddHttpClient<FullscriptHttpApiClient>();
            builder.Services.AddHttpClient<FullscriptOAuthDiagnosticService>();
            builder.Services.AddScoped<IFullscriptApiClient>(provider =>
            {
                FullscriptApiSettings settings =
                    provider.GetRequiredService<IOptions<FullscriptApiSettings>>().Value;

                if (string.Equals(settings.ClientMode, "Http", StringComparison.OrdinalIgnoreCase))
                {
                    return provider.GetRequiredService<FullscriptHttpApiClient>();
                }

                return new StubFullscriptApiClient();
            });
            builder.Services.AddScoped<FullscriptTransactionDispatcherService>();

            builder.Services.AddHostedService<Worker>();

            return builder.Build();
        }
    }
}