using AutoMapper;

namespace CatsShelter.Service.Features.Adoption.Services.Mapping;

public class CatProfile : Profile
{
    public CatProfile()
    {
        CreateMap<Domain.Entities.Cat, Proto.Cat>();
        CreateMap<Proto.Cat, Domain.Entities.Cat>();
    }
}