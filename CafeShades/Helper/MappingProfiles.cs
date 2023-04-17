using AutoMapper;
using Cafeshades.Models.Dtos;
using CafeShades.Models.Dtos;
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
                .ForMember(dest => dest.productImage, o => o.MapFrom<ProductImageUrlResolver>())
                .ForMember(dest => dest.productCategoryId, o => o.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.productCategory, o => o.MapFrom(src => src.Category.Name))
                .ReverseMap();

            CreateMap<OrderItem, ProductDto>()
                .ForMember(dest => dest.productName, o => o.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.productId, o => o.MapFrom(src => src.Product.Id))
                .ForMember(dest => dest.productPrice, o => o.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.productImage, o => o.MapFrom<ProductImageUrlFromOrderItem>())
                .ForMember(dest => dest.productCategoryId, o => o.MapFrom(src => src.Product.CategoryId))
                .ForMember(dest => dest.productCategory, o => o.MapFrom(src => src.Product.Category.Name))
                .ReverseMap();

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.productName, o => o.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.productId, o => o.MapFrom(src => src.Product.Id))
                .ForMember(dest => dest.productPrice, o => o.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.productQuantity, o => o.MapFrom(src => src.Quantity))
                .ReverseMap();

            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CategoryImage, opt => opt.MapFrom<CategoryImageUrlResolver>())
                .ForMember(dest => dest.ProductList, opt => opt.MapFrom(src => src.Products))
                .ReverseMap();

            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.userId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.orderStatus, opt => opt.MapFrom(src => src.OrderStatus.StatusName))
                .ForMember(dest => dest.orderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.orderDate, opt => opt.MapFrom(src => src.Date))
                .ForMember(dest => dest.productList, opt => opt.MapFrom(src => src.OrderItems))
                .ReverseMap();


            CreateMap<User, UserDto>()
                .ForMember(dest => dest.userId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.buildingName, opt => opt.MapFrom(src => src.BuildingName))
                .ForMember(dest => dest.floorNumber, opt => opt.MapFrom(src => src.FloorNumber))
                .ForMember(dest => dest.landmark, opt => opt.MapFrom(src => src.Landmark))
                .ForMember(dest => dest.mobileNumber, opt => opt.MapFrom(src => src.MobileNumber))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.officeNumber, opt => opt.MapFrom(src => src.OfficeNumber))
                .ReverseMap();
                
        }
    }
}
