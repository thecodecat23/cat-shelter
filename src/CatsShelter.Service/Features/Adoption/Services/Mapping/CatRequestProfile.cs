using AutoMapper;

namespace CatsShelter.Service.Features.Adoption.Services.Mapping;

public class CatRequestProfile : Profile
{
    public CatRequestProfile()
    {
        CreateMap<CatAdoptionRequest, Proto.CatRequest>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CatId));

        CreateMap<Proto.CatRequest, CatAdoptionRequest>()
            .ForMember(dest => dest.CatId, opt => opt.MapFrom(src => src.Id));
    }
}