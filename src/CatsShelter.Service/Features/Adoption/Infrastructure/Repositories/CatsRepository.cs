using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories.Exceptions;

namespace CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;

public class CatsRepository : ICatsRepository
{
    private readonly ICatsDatabaseContext _context;

    public CatsRepository(ICatsDatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<Cat>> GetAvailableCatsAsync(CancellationToken cancellationToken) =>
        await _context.GetAvailableCatsAsync(cancellationToken);

    public async Task<Cat> GetCatByIdAsync(string id, CancellationToken cancellationToken)
    {
        var cat = await _context.FindCatAsync(id, cancellationToken);

        if (cat is null)
            throw new CatNotFoundException(id);

        return cat;
    }

    public async Task UpdateCatAsync(Cat cat, CancellationToken cancellationToken)
    {
        var replaceResult = await _context.ReplaceOneAsync(cat, cancellationToken);

        if (!replaceResult.IsAcknowledged)
            throw new CatUpdateException(cat.Id);

        if (replaceResult.ModifiedCount == 0)
            throw new CatNotFoundException(cat.Id);
    }
}