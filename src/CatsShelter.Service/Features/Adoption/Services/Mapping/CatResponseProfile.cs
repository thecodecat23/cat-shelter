using AutoMapper;

namespace CatsShelter.Service.Features.Adoption.Services.Mapping;

public class CatResponseProfile : Profile
{
    public CatResponseProfile()
    {
        CreateMap<CatAdoptionResponse, Proto.AdoptionResponse>();
        CreateMap<Proto.AdoptionResponse, CatAdoptionResponse>();
    }
}