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
}