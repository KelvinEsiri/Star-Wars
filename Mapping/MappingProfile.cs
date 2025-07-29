using AutoMapper;
using Star_Wars.DTOs;
using Star_Wars.Models;
using Star_Wars.Models.SWAPI;

namespace Star_Wars.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Starship mappings
            CreateMap<Starship, StarshipDto>();
            CreateMap<CreateStarshipDto, Starship>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Created, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Edited, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Url, opt => opt.Ignore());

            CreateMap<UpdateStarshipDto, Starship>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Created, opt => opt.Ignore())
                .ForMember(dest => dest.Edited, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Url, opt => opt.Ignore());

            // SWAPI to Starship mapping
            CreateMap<SwapiStarship, Starship>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => 
                    src.Manufacturer != null && src.Manufacturer.Length > 200 
                        ? src.Manufacturer.Substring(0, 200) 
                        : src.Manufacturer));
        }
    }
}
