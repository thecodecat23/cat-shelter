using CatsShelter.Service.Features.Adoption.Domain.Entities;

namespace CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;

public interface ICatsRepository
{
    Task<Cat> GetCatByIdAsync(string id, CancellationToken cancellationToken);
    Task UpdateCatAsync(Cat cat, CancellationToken cancellationToken);
    Task<List<Cat>> GetAvailableCatsAsync(CancellationToken cancellationToken);
}
