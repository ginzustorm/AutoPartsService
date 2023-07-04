using AutoMapper;
using AutoPartsServiceWebApi.Dto;
using AutoPartsServiceWebApi.Models;

namespace AutoPartsServiceWebApi
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LoginSmsDto, LoginSms>();
            CreateMap<UserCommon, UserCommonDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<Car, CarDto>();
            CreateMap<Car, ResponseCarDto>();
            //CreateMap<Service, ServiceDto>();
            CreateMap<RequestCategory, RequestCategoryDto>();
            CreateMap<Request, CreateRequestDto>();
            CreateMap<CreateRequestDto, Request>();
            CreateMap<Request, RequestDto>();
            CreateMap<Offer, OfferDto>();
            CreateMap<UserCommon, UserCredentialsDto>();
            //CreateMap<Service, ServiceWithUserDto>();
            CreateMap<Review, ReviewDto>();
            CreateMap<ReviewDto, Review>();
            CreateMap<Service, ServiceDto>()
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews));
        }
    }

}
