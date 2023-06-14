using CatsShelter.Service.Features.Adoption.Domain.Entities;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;

namespace CatsShelter.Service.Features.Adoption.Services;

public class CatsAdoptionService : ICatsAdoptionService
{
    private readonly ICatsRepository _catsRepository;

    public CatsAdoptionService(
        ICatsRepository catsRepository
    )
    {
        _catsRepository = catsRepository;
    }

    public async Task<IEnumerable<Cat>> GetAvailableCatsAsync(CancellationToken cancellationToken) =>
        await _catsRepository.GetAvailableCatsAsync(cancellationToken);

    public async Task<CatAdoptionResponse> RequestAdoptionAsync(CatAdoptionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cat = await _catsRepository.GetCatByIdAsync(request.CatId, cancellationToken);

            cat.RequestAdoption();

            await _catsRepository.UpdateCatAsync(cat, cancellationToken);
        }
        catch (Exception exception)
        {
            return new FailCatAdoptionResponse(exception);
        }

        return new SuccessRequestCatAdoptionResponse();
    }

    public async Task<CatAdoptionResponse> CancelAdoptionAsync(CatAdoptionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var cat = await _catsRepository.GetCatByIdAsync(request.CatId, cancellationToken);

            cat.CancelAdoption();

            await _catsRepository.UpdateCatAsync(cat, cancellationToken);
        }
        catch (Exception exception)
        {
            return new FailCatAdoptionResponse(exception);
        }

        return new SuccessCancelCatAdoptionResponse();
    }
}