using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Marketplace.IntegrationTests.Controllers;

public class AdditionalControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AdditionalControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductReviews_ShouldReturnOk()
    {
        var response = await _client.GetAsync($"/api/reviews/product/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProtectedCartEndpoint_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/cart");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
