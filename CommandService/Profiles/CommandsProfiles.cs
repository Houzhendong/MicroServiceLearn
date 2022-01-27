using AutoMapper;
using CommandService.Dtos;
using CommandService.Models;
using PlatformService;

namespace CommandService.Profiles
{
    public class CommandsProfiles : Profile
    {
        public CommandsProfiles()
        {
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<CommandCreateDto, Command>().ReverseMap();
            CreateMap<PlatformPublishedDto, Platform>().ForMember(dst => dst.ExternalId, opt => opt.MapFrom(src => src.Id));
            CreateMap<GrpcPlatformModel, Platform>()
                .ForMember(dst => dst.Name, opt => opt.MapFrom(dst => dst.Name))
                .ForMember(dst => dst.ExternalId, opt => opt.MapFrom(src => src.PlatformId))
                .ForMember(dst => dst.Commands, opt => opt.Ignore());
        }
    }
}