using AutoMapper;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Features.Users.Query;
using CleanArchitecture.Entites.Entites;

namespace BE_2911_CleanArchitechture.Handler
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<TblCustomer, CustomerEntity>().ForMember(item => item.StatusName, item => item.MapFrom(s => s.IsActive == true ? "Active" : "In Active"));
            CreateMap<ProductsDto, Product>().ReverseMap();
            CreateMap<LoginQuery, UsersDto>();
            CreateMap<UsersDto, User>().ReverseMap();
            CreateMap<ReviewsDto, Review>().ReverseMap();
            CreateMap<CommentsDto, Comment>().ReverseMap();
            CreateMap<CommentsDto, Comment>().ReverseMap();
            CreateMap<FriendsDto, Friend>().ReverseMap();

            // Map User -> UserWithDetailDto excluding sensitive fields like PasswordHash, Email, Token, Role
            CreateMap<User, UserWithDetailDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.UserDetail != null ? src.UserDetail.BirthDate : null))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.UserDetail != null ? src.UserDetail.Address : null))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.UserDetail != null ? src.UserDetail.Bio : null))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src =>  src.UserDetail.Gender ))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src =>  src.UserDetail.CountryCode ))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src =>  src.UserDetail.Phone ))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src =>  src.UserDetail.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.UserDetail.LastName))
                .ForMember(dest => dest.Material, opt => opt.MapFrom(src =>  src.UserDetail.Material))
                .ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<UserDetail, UserWithDetailDto>()
                // 1. Ánh xạ các trường từ chính UserDetail
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.Bio))
                .ForMember(dest => dest.BirthDate, opt => opt.MapFrom(src => src.BirthDate))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                // Xử lý null cho các kiểu int (nếu null thì mặc định về 0)
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender ?? 0))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CountryCode ?? 0))
                .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.Material ?? 0))

                .ForMember(dest => dest.FriendCount, opt => opt.Ignore());
        }

    }
}
