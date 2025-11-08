using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using FreelancerAssignment.DTOs.Products;

namespace FreelancerAssignment.IntegrationTests.Products;

public class ProductCategoryEndpointTests(FreelanceAssignmentWebApplicationFactory factory) 
    : IClassFixture<FreelanceAssignmentWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _baseUrl = "/api/products";

    [Fact]
    public async Task GetAllCategories_ShouldReturn_Ok()
    {
        var response = await _client.GetAsync($"{_baseUrl}/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<string>>();
        categories.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductByCategory_ShouldReturn_Ok()
    {
        var category = "Electronics";
        var response = await _client.GetAsync($"{_baseUrl}/by-category/{category}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductByCategory_ShouldReturn_Ok_WhenCategoryEmpty()
    {
        var category = "NonExistentCategory";
        var response = await _client.GetAsync($"{_baseUrl}/by-category/{category}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        products.Should().NotBeNull();
        products.Should().BeEmpty();
    }
}