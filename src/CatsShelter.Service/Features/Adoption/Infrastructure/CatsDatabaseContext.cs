using Bogus;
using CatsShelter.Service.Features.Adoption.Domain.Entities;
using MongoDB.Driver;

namespace CatsShelter.Service.Features.Adoption.Infrastructure;

public class CatsDatabaseContext : ICatsDatabaseContext
{
    private readonly IMongoCollection<Cat> _cats;

    public CatsDatabaseContext(
        IMongoClient client,
        string databaseName,
        string collectionName)
    {
        var database = client.GetDatabase(databaseName);
        _cats = database.GetCollection<Cat>(collectionName);
    }

    public async Task<List<Cat>> GetAvailableCatsAsync(CancellationToken cancellationToken)
    {
        var filter = Builders<Cat>.Filter.Eq(c => c.IsAvailable, true);
        return await _cats.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task<Cat> FindCatAsync(string id, CancellationToken cancellationToken)
    {
        var filter = Builders<Cat>.Filter.Eq(c => c.Id, id);
        return await _cats.Find(filter).SingleAsync(cancellationToken);
    }

    public async Task<CatUpdateResult> ReplaceOneAsync(Cat cat, CancellationToken cancellationToken)
    {
        var filter = Builders<Cat>.Filter.Eq(c => c.Id, cat.Id);
        var result = await _cats.ReplaceOneAsync(filter, cat, (ReplaceOptions)null!, cancellationToken);
        return new CatUpdateResult
        {
            IsAcknowledged = result.IsAcknowledged,
            ModifiedCount = result.ModifiedCount
        };
    }

    public async Task SeedDatabase(int catsToSeed, CancellationToken cancellationToken)
    {
        var faker = new Faker<Cat>()
            .CustomInstantiator(f => new Cat(f.Random.Guid().ToString(), f.Name.FirstName()));

        await _cats.InsertManyAsync(faker.Generate(catsToSeed), default(InsertManyOptions), cancellationToken);
    }
}