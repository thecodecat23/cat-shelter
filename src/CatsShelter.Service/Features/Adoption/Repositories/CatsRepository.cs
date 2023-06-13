using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Repositories.Exceptions;
using MongoDB.Driver;

namespace CatsShelter.Service.Features.Adoption.Repositories;

public class CatsRepository : ICatsRepository
{
    private readonly IMongoCollection<Cat> _cats;

    public CatsRepository(IMongoClient client, string databaseName, string collectionName)
    {
        var database = client.GetDatabase(databaseName);
        _cats = database.GetCollection<Cat>(collectionName);
    }

    public async Task<Cat> GetCatByIdAsync(string id, CancellationToken cancellationToken)
    {
        var cat = await _cats.FindAsync(cat => cat.Id == id, default(FindOptions<Cat>), cancellationToken);

        if (cat is null)
            throw new CatNotFoundException(id);

        return await cat.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpdateCatAsync(Cat cat, CancellationToken cancellationToken)
    {
        var replaceResult = await _cats.ReplaceOneAsync(c => c.Id == cat.Id, cat, (ReplaceOptions)null!, cancellationToken);

        if (!replaceResult.IsAcknowledged || replaceResult.ModifiedCount == 0)
            throw new CatNotFoundException(cat.Id);
    }
}