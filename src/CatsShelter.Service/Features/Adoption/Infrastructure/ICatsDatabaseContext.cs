using CatsShelter.Service.Features.Adoption.Domain.Entities;

namespace CatsShelter.Service.Features.Adoption.Infrastructure;

public interface ICatsDatabaseContext
{
    Task<Cat> FindCatAsync(string id, CancellationToken cancellationToken);
    Task<CatUpdateResult> ReplaceOneAsync(Cat cat, CancellationToken cancellationToken);
    Task<List<Cat>> GetAvailableCatsAsync(CancellationToken cancellationToken);
    Task SeedDatabase(int catsToSeed, CancellationToken cancellationToken);
}