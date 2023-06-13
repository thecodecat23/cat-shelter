using CatsShelter.Service.Features.Adoption.Domain.Entities;

namespace CatsShelter.Service.Features.Adoption.Repositories;

public interface ICatsRepository
{
    Task<Cat> GetCatByIdAsync(string id);
    Task<bool> UpdateCatAsync(Cat cat);
}
