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

                    services.AddHostedService<Worker>();
                });
        }
    }
}