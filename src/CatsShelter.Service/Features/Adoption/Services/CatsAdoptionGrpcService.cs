using AutoMapper;
using CatsShelter.Service.Features.Adoption.Proto;
using Grpc.Core;

namespace CatsShelter.Service.Features.Adoption.Services;

public class CatsAdoptionGrpcService : CatsShelterService.CatsShelterServiceBase
{
    private readonly ICatsAdoptionService _catsAdoptionService;
    private readonly IMapper _mapper;

    public CatsAdoptionGrpcService(
        ICatsAdoptionService catsAdoptionService,
        IMapper mapper
    )
    {
        _catsAdoptionService = catsAdoptionService;
        _mapper = mapper;
    }

    public override async Task<AdoptionResponse> RequestAdoption(CatRequest request, ServerCallContext context)
    {
        var catRequestAdoptionRequest = _mapper.Map<CatAdoptionRequest>(request);
        
        var catRequestAdoptionResponse = await _catsAdoptionService.RequestAdoptionAsync(catRequestAdoptionRequest, context.CancellationToken);

        return _mapper.Map<AdoptionResponse>(catRequestAdoptionResponse);
    }

    public override async Task<Cats> GetAvailableCats(Empty request, ServerCallContext context)
    {        
        var domainCats = await _catsAdoptionService.GetAvailableCatsAsync(context.CancellationToken);

        return new Cats
        {
            Cats_ = { _mapper.Map<List<Proto.Cat>>(domainCats) }
        };
    }

    public override async Task<AdoptionResponse> CancelAdoption(CatRequest request, ServerCallContext context)
    {
        var catCancelAdoptionRequest = _mapper.Map<CatAdoptionRequest>(request);

        var catCancelAdoptionResponse = await _catsAdoptionService.CancelAdoptionAsync(catCancelAdoptionRequest, context.CancellationToken);

        return _mapper.Map<AdoptionResponse>(catCancelAdoptionResponse);
    }
}