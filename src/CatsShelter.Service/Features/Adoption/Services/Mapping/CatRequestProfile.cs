using AutoMapper;

namespace CatsShelter.Service.Features.Adoption.Services.Mapping;

public class CatRequestProfile : Profile
{
    public CatRequestProfile()
    {
        CreateMap<CatAdoptionRequest, Proto.CatRequest>();
        CreateMap<Proto.CatRequest, CatAdoptionRequest>();
    }
}