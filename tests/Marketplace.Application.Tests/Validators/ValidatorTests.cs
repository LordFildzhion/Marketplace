using FluentAssertions;
using FluentValidation;
using Marketplace.Application.DTOs.Auth;
using Marketplace.Application.DTOs.Cart;
using Marketplace.Application.DTOs.Categories;
using Marketplace.Application.DTOs.Orders;
using Marketplace.Application.DTOs.Products;
using Marketplace.Application.DTOs.Reviews;
using Marketplace.Application.DTOs.Users;
using Marketplace.Application.Validators.Auth;
using Marketplace.Application.Validators.Cart;
using Marketplace.Application.Validators.Orders;
using Marketplace.Application.Validators.Payments;
using Marketplace.Application.Validators.Products;
using Marketplace.Application.Validators.Reviews;
using Marketplace.Application.Validators.Users;
using Xunit;

namespace Marketplace.Application.Tests.Validators;

public class ValidatorTests
{
    private static void ShouldBeValid<T>(IValidator<T> validator, T model) => validator.Validate(model).IsValid.Should().BeTrue();
    private static void ShouldBeInvalid<T>(IValidator<T> validator, T model) => validator.Validate(model).IsValid.Should().BeFalse();

    [Fact] public void Login_Valid() => ShouldBeValid(new LoginRequestValidator(), new LoginRequest("user@example.com", "pass"));
    [Fact] public void Login_InvalidEmail() => ShouldBeInvalid(new LoginRequestValidator(), new LoginRequest("bad", "pass"));

    [Fact] public void Register_Valid() => ShouldBeValid(new RegisterRequestValidator(), new RegisterRequest { Email="u@example.com", Password="12345678", ConfirmPassword="12345678", FirstName="A", LastName="B" });
    [Fact] public void Register_MismatchedPassword() => ShouldBeInvalid(new RegisterRequestValidator(), new RegisterRequest { Email="u@example.com", Password="12345678", ConfirmPassword="87654321", FirstName="A", LastName="B" });

    [Fact] public void AddToCart_Valid() => ShouldBeValid(new AddToCartValidator(), new AddToCartRequest { ProductId=Guid.NewGuid(), Quantity=2 });
    [Fact] public void AddToCart_ZeroQuantity() => ShouldBeInvalid(new AddToCartValidator(), new AddToCartRequest { ProductId=Guid.NewGuid(), Quantity=0 });
    [Fact] public void UpdateCart_ZeroQuantity_IsValidByContract() => ShouldBeValid(new UpdateCartItemValidator(), new UpdateCartItemRequest { ProductId=Guid.NewGuid(), Quantity=0 });

    [Fact] public void UpdateOrderStatus_Valid() => ShouldBeValid(new UpdateOrderStatusValidator(), new UpdateOrderStatusRequest { NewStatus="Shipped" });
    [Fact] public void UpdateOrderStatus_Invalid() => ShouldBeInvalid(new UpdateOrderStatusValidator(), new UpdateOrderStatusRequest { NewStatus="Draft" });

    [Fact] public void Payment_Valid() => ShouldBeValid(new ProcessPaymentValidator(), new ProcessPaymentRequest { OrderId=Guid.NewGuid() });
    [Fact] public void Payment_MissingOrder() => ShouldBeInvalid(new ProcessPaymentValidator(), new ProcessPaymentRequest());

    [Fact] public void CreateProduct_Valid() => ShouldBeValid(new CreateProductValidator(), new CreateProductRequest { Title="Phone", Price=10, Stock=2, CategoryId=Guid.NewGuid() });
    [Fact] public void CreateProduct_InvalidPrice() => ShouldBeInvalid(new CreateProductValidator(), new CreateProductRequest { Title="Phone", Price=0, Stock=2, CategoryId=Guid.NewGuid() });
    [Fact] public void UpdateProduct_EmptyModel_IsValid() => ShouldBeValid(new UpdateProductValidator(), new UpdateProductRequest());
    [Fact] public void UpdateProduct_NegativeStock() => ShouldBeInvalid(new UpdateProductValidator(), new UpdateProductRequest { Stock=-1 });
    [Fact] public void SearchProduct_Valid() => ShouldBeValid(new ProductSearchRequestValidator(), new ProductSearchRequest { Page=1, PageSize=20 });
    [Fact] public void SearchProduct_InvalidPageSize() => ShouldBeInvalid(new ProductSearchRequestValidator(), new ProductSearchRequest { Page=1, PageSize=101 });
    [Fact] public void SearchProduct_NegativeMaxPrice() => ShouldBeInvalid(new ProductSearchRequestValidator(), new ProductSearchRequest { Page=1, PageSize=20, MaxPrice=-1 });

    [Fact] public void CreateReview_Valid() => ShouldBeValid(new CreateReviewValidator(), new CreateReviewRequest { Rating=5, Comment="Great" });
    [Fact] public void CreateReview_InvalidRating() => ShouldBeInvalid(new CreateReviewValidator(), new CreateReviewRequest { Rating=6, Comment="Great" });
    [Fact] public void UpdateReview_EmptyModel_IsValid() => ShouldBeValid(new UpdateReviewValidator(), new UpdateReviewRequest());

    [Fact] public void UpdateProfile_Valid() => ShouldBeValid(new UpdateProfileValidator(), new UpdateProfileRequest { FirstName="John", Phone="+49123456789" });
    [Fact]
    public void UpdateProfile_InvalidPhone_ShouldRejectLetters()
    {
        ShouldBeInvalid(
            new UpdateProfileValidator(),
            new UpdateProfileRequest
            {
                Phone = "+49abc123"
            });
    }

    [Fact]
    public void UpdateProfile_ValidPhone_ShouldAcceptE164()
    {
        ShouldBeValid(
            new UpdateProfileValidator(),
            new UpdateProfileRequest
            {
                Phone = "+49123456789"
            });
    }
}
