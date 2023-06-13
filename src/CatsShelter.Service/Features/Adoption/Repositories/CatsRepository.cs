using CatsShelter.Service.Features.Adoption.Domain.Entities;
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

    public Task<Cat> GetCatByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateCatAsync(Cat cat)
    {
        throw new NotImplementedException();
    }
}