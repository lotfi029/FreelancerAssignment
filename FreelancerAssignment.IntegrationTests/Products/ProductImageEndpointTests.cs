using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace FreelancerAssignment.IntegrationTests.Products;

public class ProductImageEndpointTests(FreelanceAssignmentWebApplicationFactory factory) 
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
    public async Task ChangeProductImage_ShouldReturn_NoContent_WhenValid()
    {
        
        var formData = CreateProductForm(
            "Test Product",
            "Test Category",
            100.0m,
            1,
            1!,
            true
        );

        var addResponse = await _client.PostAsync(_baseUrl, formData);
        var location = addResponse.Headers.Location?.ToString();
        var productId = Guid.Parse(location!.Split('/').Last());
        _createdProductIds.Add(productId);

        // Add an image
        var image = new ByteArrayContent(Encoding.UTF8.GetBytes("fake-image-content"));
        image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var imageForm = new MultipartFormDataContent
        {
            { image, "image", "photo.png" }
        };

        var response = await _client.PostAsync($"{_baseUrl}/{productId}/product-image", imageForm);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetProductImage_ShouldReturn_NotFound_WhenNotExists()
    {
        var id = Guid.NewGuid();
        var response = await _client.GetAsync($"{_baseUrl}/{id}/product-image");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductImage_ShouldReturn_Ok_WhenExists()
    {
        var formData = CreateProductForm(
            "Test Product",
            "Test Category",
            100.0m,
            1,
            1!,
            true
        );

        var addResponse = await _client.PostAsync(_baseUrl, formData);
        var location = addResponse.Headers.Location?.ToString();
        var productId = Guid.Parse(location!.Split('/').Last());
        _createdProductIds.Add(productId);

        var image = new ByteArrayContent(Encoding.UTF8.GetBytes("fake-image-content"));
        image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var imageForm = new MultipartFormDataContent
        {
            { image, "image", "test.png" }
        };

        await _client.PostAsync($"{_baseUrl}/{productId}/product-image", imageForm);

        var response = await _client.GetAsync($"{_baseUrl}/{productId}/product-image");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
        var imageContent = await response.Content.ReadAsByteArrayAsync();
        imageContent.Should().NotBeEmpty();
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