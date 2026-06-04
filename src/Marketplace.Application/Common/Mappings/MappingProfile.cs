using AutoMapper;
using Marketplace.Application.DTOs.Products;
using Marketplace.Application.DTOs.Orders;
using Marketplace.Application.DTOs.Reviews;
using Marketplace.Application.DTOs.Users;
using Marketplace.Application.DTOs.Categories;
using Marketplace.Application.DTOs.Cart;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.DiscountPrice, o => o.MapFrom(s => s.DiscountPrice != null ? s.DiscountPrice.Amount : (decimal?)null))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : ""))
            .ForMember(d => d.ImageUrls, o => o.MapFrom(s => s.Images.Select(i => i.Url)))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images))
            .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => (int)r.Rating.Value) : 0.0))
            .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.Reviews.Count))
            .ForMember(d => d.SellerId, o => o.MapFrom(s => s.SellerId))
            .ForMember(d => d.SellerName, o => o.MapFrom(s => s.Seller != null ? s.Seller.FullName : "Неизвестный продавец"));

        CreateMap<ProductImage, ImageDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.UserEmail, o => o.MapFrom(s => s.User != null ? s.User.Email.Value : ""))
            .ForMember(d => d.TotalAmount, o => o.MapFrom(s => s.TotalAmount.Amount))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items))
            .ForMember(d => d.BuyerName, o => o.MapFrom(s => s.User != null ? s.User.FullName : ""))
            .ForMember(d => d.BuyerEmail, o => o.MapFrom(s => s.User != null ? s.User.Email.Value : ""))
            .AfterMap((src, dest) =>
            {
                if (src.Items != null)
                {
                    var sellers = src.Items
                        .Where(i => i.Product?.Seller != null)
                        .Select(i => i.Product!.Seller!.FullName)
                        .Distinct();
                    dest.SellerNames = string.Join(", ", sellers);
                }
            });

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice.Amount))
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.TotalPrice.Amount))
            .ForMember(d => d.ProductTitle, o => o.MapFrom(s => s.ProductTitle))
            .ForMember(d => d.Product, o => o.MapFrom(s => s.Product != null ? new ProductBriefDto
            {
                Id = s.Product.Id,
                Title = s.Product.Title,
                ImageUrls = s.Product.Images.Select(i => i.Url).ToList(),
                SellerName = s.Product.Seller != null ? s.Product.Seller.FullName : ""
            } : null));

        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.Rating, o => o.MapFrom(s => s.Rating.Value))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.FullName : "Unknown"))
            .ForMember(d => d.SellerResponse, o => o.MapFrom(s => s.SellerResponse))
            .ForMember(d => d.ResponseDate, o => o.MapFrom(s => s.ResponseDate))
            .ForMember(d => d.RespondedBy, o => o.MapFrom(s => s.RespondedBy));

        CreateMap<User, UserDto>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.Value))
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ParentCategoryId, o => o.MapFrom(s => s.ParentCategoryId));

        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.Product != null ? s.Product.CurrentPrice.Amount : 0m))
            .ForMember(d => d.ProductTitle, o => o.MapFrom(s => s.Product != null ? s.Product.Title : ""))
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.Product != null ? s.Product.CurrentPrice.Amount * s.Quantity : 0m));
    }
}
