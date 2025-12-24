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
                .ForAllOtherMembers(opt => opt.Ignore());
            //CreateMap<TblSalesProductInfo, InvoiceDetail>().ReverseMap();
            //CreateMap<TblProduct, ProductEntity>().ReverseMap();
            //CreateMap<TblProductvarinat, ProductVariantEntity>().ReverseMap();
            //CreateMap<TblMastervariant, VariantEntity>().ReverseMap();
            //CreateMap<TblCategory, CategoryEntity>().ReverseMap();
            //CreateMap<TblSalesHeader, InvoiceInput>().ReverseMap();
        }

    }
}
