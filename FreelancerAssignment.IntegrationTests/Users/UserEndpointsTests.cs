using FluentAssertions;
using FreelancerAssignment.DTOs.Users;
using System.Net;
using System.Net.Http.Json;

namespace FreelancerAssignment.IntegrationTests.Users;

public class UserEndpointsTests(FreelanceAssignmentWebApplicationFactory factory)
    : IClassFixture<FreelanceAssignmentWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly string _baseUrl = "/api/users";
    [Fact]
    public async Task GetProfile_WithAuth_ShouldReturnOk()
    {
        var response = await _client.GetAsync($"{_baseUrl}/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<UserResponse>();
        profile.Should().NotBeNull();
        profile!.Id.Should().Be(Consts.TestUser.Id);
        profile.Username.Should().Be(Consts.TestUser.UserName);
        profile.Email.Should().Be(Consts.TestUser.Email);
    }
}