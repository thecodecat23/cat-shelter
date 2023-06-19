using AutoMapper;

namespace CatsShelter.Service.Features.Adoption.Services.Mapping;

public class CatResponseProfile : Profile
{
    public CatResponseProfile()
    {
        CreateMap<CatAdoptionResponse, Proto.AdoptionResponse>()
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.IsSuccess));

        CreateMap<Proto.AdoptionResponse, CatAdoptionResponse>()
            .ForMember(dest => dest.IsSuccess, opt => opt.MapFrom(src => src.Success));
    }
}