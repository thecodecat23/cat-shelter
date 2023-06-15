using CatsShelter.Service.Features.Adoption.Infrastructure;

namespace CatsShelter.Service;

public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Database seeding
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ICatsDatabaseContext>();
            var cancellationTokenSource = new CancellationTokenSource(host.Services.GetRequiredService<IConfiguration>().GetValue<int>("MaxTimeoutDbSeedMilliseconds"));
            context.SeedDatabase(host.Services.GetRequiredService<IConfiguration>().GetValue<int>("CatsToSeed"), cancellationTokenSource.Token).GetAwaiter().GetResult();
        }

        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
       Host.CreateDefaultBuilder(args)
           .ConfigureAppConfiguration((hostingContext, config) =>
           {
               config.AddJsonFile("appsettings.json", optional: true)
                     .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                     .AddEnvironmentVariables();
           })
           .ConfigureWebHostDefaults(webBuilder =>
           {
               webBuilder.UseStartup<Startup>();
           });
}