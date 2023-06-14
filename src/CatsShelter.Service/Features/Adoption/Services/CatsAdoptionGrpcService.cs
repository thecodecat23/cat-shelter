using CatsShelter.Service.Features.Adoption.Proto;
using Grpc.Core;

namespace CatsShelter.Service.Features.Adoption.Services;

public class CatsAdoptionGrpcService : CatsShelterService.CatsShelterServiceBase
{
    private readonly ICatsAdoptionService _catsAdoptionService;

    public CatsAdoptionGrpcService(
        ICatsAdoptionService catsAdoptionService
    )
    {
        _catsAdoptionService = catsAdoptionService;
    }

    public override async Task<AdoptionResponse> CancelAdoption(CatRequest request, ServerCallContext context)
    {
        return await _catsAdoptionService.CancelAdoption(request);
    }

    public override async Task<Cats> GetAvailableCats(Empty request, ServerCallContext context)
    {
        return await _catsAdoptionService.GetAvailableCats(request);
    }

    public override async Task<AdoptionResponse> RequestAdoption(CatRequest request, ServerCallContext context)
    {
        return await _catsAdoptionService.RequestAdoption(request);
    }
}