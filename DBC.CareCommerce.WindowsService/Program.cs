using System;
using DBC.CareCommerce.Application.Services;
using DBC.CareCommerce.Contracts.Repositories;
using DBC.CareCommerce.Contracts.Services;
using DBC.CareCommerce.Contracts.Services.Contracts;
using DBC.CareCommerce.Data.DataAccess;
using DBC.CareCommerce.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DBC.CareCommerce.WindowsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<CareCommerceServiceSettings>(
                        hostContext.Configuration.GetSection("CareCommerceService"));

                    services.AddSingleton(provider =>
                    {
                        CareCommerceServiceSettings settings =
                            hostContext.Configuration
                                .GetSection("CareCommerceService")
                                .Get<CareCommerceServiceSettings>() ?? new CareCommerceServiceSettings();

                        string connectionString =
                            Environment.GetEnvironmentVariable("DBC_CARECOMMERCE_SQL_CONNECTION");

                        if (string.IsNullOrWhiteSpace(connectionString))
                        {
                            connectionString = settings.SqlConnectionString;
                        }

                        if (string.IsNullOrWhiteSpace(connectionString))
                        {
                            connectionString =
                                hostContext.Configuration["CareCommerceService:SqlConnectionString"];
                        }

                        return new SqlConnectionFactory(connectionString);
                    });

                    services.AddScoped<ICatalogItemRepository, SqlCatalogItemRepository>();
                    services.AddScoped<ICareItemRepository, SqlCareItemRepository>();
                    services.AddScoped<IPendingChargeRepository, SqlPendingChargeRepository>();
                    services.AddScoped<IFullscriptTransactionRepository, SqlFullscriptTransactionRepository>();

                    services.AddScoped<ICareItemApplicationService, CareItemApplicationService>();
                    services.AddScoped<ICareCommerceIntegrationService, CareCommerceIntegrationService>();
                    services.AddScoped<FullscriptTransactionDispatcherService>();

                    services.AddHostedService<Worker>();
                });
        }
    }
}