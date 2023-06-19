using CatsShelter.Service.Features.Adoption.Domain.Entities;

namespace CatsShelter.Service.Features.Adoption.Services;

public interface ICatsAdoptionService
{
    Task<IEnumerable<Cat>> GetAvailableCatsAsync(CancellationToken cancellationToken);
    Task<CatAdoptionResponse> RequestAdoptionAsync(CatAdoptionRequest request, CancellationToken cancellationToken);
    Task<CatAdoptionResponse> CancelAdoptionAsync(CatAdoptionRequest request, CancellationToken cancellationToken);
}