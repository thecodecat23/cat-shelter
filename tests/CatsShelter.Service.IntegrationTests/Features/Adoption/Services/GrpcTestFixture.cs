using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.Extensions.Hosting;

namespace CatsShelter.Service.IntegrationTests.Features.Adoption.Services;

public class GrpcTestFixture<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var mongoDbRunner = MongoDbRunner.Start();
        var mongoDbConnectionString = mongoDbRunner.ConnectionString;

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MongoDbConnection", mongoDbConnectionString },
                { "DatabaseName", "TestDb" },
                { "CollectionName", "TestCollection" }
            });
        })
        .ConfigureServices(services =>
        {
            services.AddSingleton(mongoDbRunner);
            services.AddScoped(provider =>
            {
                var mongoClient = new MongoClient(mongoDbRunner.ConnectionString);
                return mongoClient.GetDatabase("TestDb");
            });
        });

        var host = builder.Build();
        host.Start();
        return host;
    }
}