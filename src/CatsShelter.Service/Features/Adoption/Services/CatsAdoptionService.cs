using AutoMapper;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;
using CatsShelter.Service.Features.Adoption.Proto;

namespace CatsShelter.Service.Features.Adoption.Services;

public class CatsAdoptionService : ICatsAdoptionService
{
    private readonly ICatsRepository _catsRepository;
    private readonly IMapper _catMapper;

    public CatsAdoptionService(
        ICatsRepository catsRepository,
        IMapper catMapper
    )
    {
        _catsRepository = catsRepository;
        _catMapper = catMapper;
    }

    public Task<AdoptionResponse> CancelAdoption(CatRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Cats> GetAvailableCats(Empty request)
    {
        throw new NotImplementedException();
    }

    public Task<AdoptionResponse> RequestAdoption(CatRequest request)
    {
        throw new NotImplementedException();
    }
}
