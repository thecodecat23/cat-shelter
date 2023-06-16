using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.Extensions.Hosting;
using CatsShelter.Service.Features.Adoption.Domain.Entities;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CatsShelter.Service.IntegrationTests.Features.Adoption.Services;

public class GrpcTestFixture<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public IMongoCollection<Cat>? CatsCollection { get; private set; }
    public IMongoDatabase? MongoDatabase { get; private set; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var mongoDbRunner = MongoDbRunner.Start();
        var mongoDbConnectionString = mongoDbRunner.ConnectionString;
        var databaseName = $"TestDb_{Guid.NewGuid()}";
        var collectionName = $"TestCollection_{Guid.NewGuid()}";

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MongoDbConnection", mongoDbConnectionString },
                { "DatabaseName", databaseName },
                { "CollectionName", collectionName }
            });
        });

        var host = builder.Build();
        host.Start();

        var mongoClient = new MongoClient(mongoDbConnectionString);
        MongoDatabase = mongoClient.GetDatabase(databaseName);
        CatsCollection = MongoDatabase.GetCollection<Cat>(collectionName);

        // Add MongoClient to the service collection
        var serviceScopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        var serviceCollection = new ServiceCollection();
        foreach (var descriptor in services.GetRequiredService<IEnumerable<ServiceDescriptor>>())
        {
            serviceCollection.Add(descriptor);
        }
        serviceCollection.AddSingleton<IMongoClient>(mongoClient);
        var newServiceProvider = serviceCollection.BuildServiceProvider();

        return host;
    }
}