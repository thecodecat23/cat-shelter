using CatsShelter.Service.Features.Adoption.Proto;

namespace CatsShelter.Service.Features.Adoption.Services;

public interface ICatsAdoptionService
{
    Task<Cats> GetAvailableCats(Empty request);
    Task<AdoptionResponse> RequestAdoption(CatRequest request);
    Task<AdoptionResponse> CancelAdoption(CatRequest request);
}