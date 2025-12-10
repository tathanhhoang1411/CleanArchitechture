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
            //CreateMap<TblSalesProductInfo, InvoiceDetail>().ReverseMap();
            //CreateMap<TblProduct, ProductEntity>().ReverseMap();
            //CreateMap<TblProductvarinat, ProductVariantEntity>().ReverseMap();
            //CreateMap<TblMastervariant, VariantEntity>().ReverseMap();
            //CreateMap<TblCategory, CategoryEntity>().ReverseMap();
            //CreateMap<TblSalesHeader, InvoiceInput>().ReverseMap();
        }

    }
}
