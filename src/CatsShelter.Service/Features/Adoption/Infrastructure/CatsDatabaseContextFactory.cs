using MongoDB.Driver;

namespace CatsShelter.Service.Features.Adoption.Infrastructure;

public class CatsDatabaseContextFactory : ICatsDatabaseContextFactory
{
    private readonly IMongoClient _mongoClient;

    public CatsDatabaseContextFactory(IMongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public ICatsDatabaseContext Create(string databaseName, string collectionName)
    {
        return new CatsDatabaseContext(_mongoClient, databaseName, collectionName);
    }
}