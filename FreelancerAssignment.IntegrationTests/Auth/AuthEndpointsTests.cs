using FluentAssertions;
using FreelancerAssignment.DTOs.Users;
using Microsoft.AspNetCore.Mvc;
using FreelancerAssignment.IntegrationTests.Consts;
using System.Net;
using System.Net.Http.Json;

namespace FreelancerAssignment.IntegrationTests.Auth;

public class AuthEndpointsTests(FreelanceAssignmentWebApplicationFactory factory)
    : IClassFixture<FreelanceAssignmentWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _baseUrl = "/api/auths";

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnNoContent()
    {
        var request = new RegisterRequest(
            Username: "newuser",
            Password: "Test123!@#",
            Email: "test@example.com"
        );

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.GetValues("Set-Cookie").Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest(
            Username: "", // Invalid empty username
            Password: "short", // Invalid short password
            Email: "not-an-email" // Invalid email format
        );

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var validationProblems = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblems.Should().NotBeNull();
        validationProblems!.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest(
            Username: "differentuser",
            Password: "Test123!@#",
            Email: TestUser.Email // Using existing email
        );

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnNoContent()
    {
        var request = new LoginRequest(
            UsernameOrEmail: "Test@user.com",
            Password: "TestPassword"
        );

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Headers.GetValues("Set-Cookie").Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
    {
        var request = new LoginRequest(
            UsernameOrEmail: "invalid@user.com",
            Password: "wrongpassword"
        );

        var response = await _client.PostAsJsonAsync($"{_baseUrl}/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task CheckAuthStatus_WithAuth_ShouldReturnNoContent()
    {
        // TestAuthHandler automatically adds authentication
        var response = await _client.GetAsync($"{_baseUrl}/check-auth-status");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RefreshToken_WithoutTokens_ShouldReturnUnauthorized()
    {
        var response = await _client.PostAsync($"{_baseUrl}/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}