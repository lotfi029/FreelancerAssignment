using FluentAssertions;
using FreelancerAssignment.DTOs.Products;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace FreelancerAssignment.IntegrationTests.Products;

public class UpdateProductEndpointTests(FreelanceAssignmentWebApplicationFactory factory) 
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
    public async Task UpdateProduct_ShouldReturn_NoContent_WhenValid()
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

        var updateRequest = new UpdateProductRequest(
            Name: "Updated Product",
            Category: "Updated Category",
            Price: 200.0m,
            MinimumQuantity: 2
        );

        var response = await _client.PutAsJsonAsync($"{_baseUrl}/{productId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturn_NotFound_WhenNotExists()
    {
        var request = new UpdateProductRequest(
            Name: "Updated Product",
            Category: "Updated Category",
            Price: 200.0m,
            MinimumQuantity: 2
        );

        var response = await _client.PutAsJsonAsync($"{_baseUrl}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturn_BadRequest_WhenInvalidRequest()
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

        var updateRequest = new ProductRequest(
            Name: "", 
            Category: "",
            Price: -1,
            MinimumQuantity: 0,
            Image: null!
        );

        var response = await _client.PutAsJsonAsync($"{_baseUrl}/{productId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationProblems = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblems.Should().NotBeNull();
        validationProblems!.Errors.Should().NotBeEmpty();
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