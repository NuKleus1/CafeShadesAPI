using AutoMapper;
using Cafeshades.Models.Dtos;
using Core.Entities;

namespace Cafeshades.Helper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.productName, o => o.MapFrom(src => src.Name))
                .ForMember(dest => dest.productId, o => o.MapFrom(src => src.Id))
                .ForMember(dest => dest.productPrice, o => o.MapFrom(src => src.Price))
                .ForMember(dest => dest.productQuantity, o => o.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.productImage, o => o.MapFrom<ProductImageUrlResolver>())
                .ForMember(dest => dest.productCategoryId, o => o.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.productCategory, o => o.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.total, o => o.MapFrom(src => src.Price * src.Quantity))
                .ReverseMap();

            CreateMap<OrderItem, ProductDto>()
                .ForMember(dest => dest.productName, o => o.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.productId, o => o.MapFrom(src => src.Product.Id))
                .ForMember(dest => dest.productPrice, o => o.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.productQuantity, o => o.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.productImage, o => o.MapFrom<ProductImageUrlFromOrderItem>())
                .ForMember(dest => dest.productCategoryId, o => o.MapFrom(src => src.Product.CategoryId))
                .ForMember(dest => dest.productCategory, o => o.MapFrom(src => src.Product.Category.Name))
                .ForMember(dest => dest.total, o => o.MapFrom(src => src.Quantity * src.Product.Price));


            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CategoryImage, opt => opt.MapFrom<CategoryImageUrlResolver>())
                .ForMember(dest => dest.ProductList, opt => opt.MapFrom(src => src.Products))
                .ReverseMap();

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.orderStatus, opt => opt.MapFrom(src => src.OrderStatus.StatusName))
                .ForMember(dest => dest.orderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.orderDate, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.productList, opt => opt.MapFrom(src => src.OrderItems));
                
        }
    }
}
