using FluentAssertions;
using FreelancerAssignment.DTOs.Products;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace FreelancerAssignment.IntegrationTests.Products;

public class GetProductEndpointTests(FreelanceAssignmentWebApplicationFactory factory) 
    : IClassFixture<FreelanceAssignmentWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _baseUrl = "/api/products";
    private readonly List<Guid> _createdProductIds = [];

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var productId in _createdProductIds)
        {
            try
            {
                await _client.DeleteAsync($"{_baseUrl}/{productId}");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturn_Ok()
    {
        var response = await _client.GetAsync(_baseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await response.Content.ReadFromJsonAsync<List<ProductResponse>>();
        data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductById_ShouldReturn_NotFound_WhenNotExists()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"{_baseUrl}/{fakeId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductById_ShouldReturn_Ok_WhenExists()
    {
        var addRequest = CreateProductForm(
            "Test Product",
            "Test Category",
            100.0m,
            1,
            1!,
            true
        );

        var addResponse = await _client.PostAsync(_baseUrl, addRequest);
        var location = addResponse.Headers.Location?.ToString();
        var productId = Guid.Parse(location!.Split('/').Last());
        _createdProductIds.Add(productId);

        // Get the product
        var response = await _client.GetAsync($"{_baseUrl}/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product.Should().NotBeNull();
        product!.Name.Should().Be("Test Product");
        product.Category.Should().Be("Test Category");
        product.Price.Should().Be(100.0m);
        product.MinimumQuantity.Should().Be(1);
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturn_NoContent_WhenExists()
    {
        var addRequest = CreateProductForm(
            "Test Product",
            "Test Category",
            100.0m,
            1,
            1!,
            true
        );

        var addResponse = await _client.PostAsync(_baseUrl, addRequest);
        var location = addResponse.Headers.Location?.ToString();
        var productId = Guid.Parse(location!.Split('/').Last());

        var response = await _client.DeleteAsync($"{_baseUrl}/{productId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    private static MultipartFormDataContent CreateProductForm(
        string name,
        string category,
        decimal price,
        int minQty,
        decimal? discount = null,
        bool includeImage = false)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(category), "Category" },
            { new StringContent(price.ToString()), "Price" },
            { new StringContent(minQty.ToString()), "MinimumQuantity" }
        };

        if (discount.HasValue)
            form.Add(new StringContent(discount.Value.ToString()), "Discount");

        if (includeImage)
        {
            var imageBytes = Encoding.UTF8.GetBytes("fake-image-content");
            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(imageContent, "Image", "photo.png");
        }

        return form;
    }
}