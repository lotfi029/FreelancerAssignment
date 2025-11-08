using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace FreelancerAssignment.IntegrationTests.Products;

public class AddProductEndpointTests(FreelanceAssignmentWebApplicationFactory factory) 
    : IClassFixture<FreelanceAssignmentWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _addUrl = "/api/products";
    
    [Fact]
    public async Task Add_WithValidRequest_ShouldReturnCreated()
    {
        var form = CreateProductForm(
            "Test Product",
            "Electronics", 
            1500, 
            2, 
            10, 
            true
            );

        var response = await _client.PostAsync(_addUrl, form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain(_addUrl);
    }

    [Fact]
    public async Task Add_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var form = CreateProductForm(
            "bla ", 
            string.Empty, 
            0, 
            0,
            0,
            true
            );

        var response = await _client.PostAsync(_addUrl, form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ValidationProblemDetails? validationProblems = null;

        if (response.Content.Headers.ContentLength > 0)
        {
            validationProblems = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        }
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