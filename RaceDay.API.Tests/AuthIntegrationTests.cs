
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay.API.DTOs;

namespace RaceDay.API.Tests;

public class AuthIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    [Fact]
    public async Task Register_WithValidDetails_ReturnsCreated()
    {
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = $"participant-{Guid.NewGuid():N}@example.com",
            Password = "RaceDayTest2026!",
            Role = "Participant"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(body);
        Assert.Equal(request.FirstName, body.FirstName);
        Assert.Equal(request.LastName, body.LastName);
        Assert.Equal(request.Email, body.Email);
        Assert.Equal("Participant", body.Role);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = $"duplicate-{Guid.NewGuid():N}@example.com";

        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = email,
            Password = "RaceDayTest2026!",
            Role = "Participant"
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register", request);

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register", request);

        Assert.Equal(
            HttpStatusCode.Created, firstResponse.StatusCode);

        Assert.Equal(
            HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var email = $"login-{Guid.NewGuid():N}@example.com";

        var registration = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = email,
            Password = "RaceDayTest2026!",
            Role = "Participant"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register", registration);

        Assert.Equal(
            HttpStatusCode.Created, registerResponse.StatusCode);

        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = "WrongPassword2026!"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login", loginRequest);

        Assert.Equal(
            HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }


    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessAndAllowsProfileAccess()
    {
        var email = $"valid-login-{Guid.NewGuid():N}@example.com";
        const string password = "RaceDayTest2026!";

        var registration = new RegisterRequest
        {
            FirstName = "Login",
            LastName = "Tester",
            Email = email,
            Password = password,
            Role = "Participant"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register", registration);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest
            {
                Email = email,
                Password = password
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var profileResponse = await _client.GetAsync("/api/Auth/me");

        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);

        var profile = await profileResponse.Content
            .ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(profile);
        Assert.Equal(email, profile.Email);
        Assert.Equal("Participant", profile.Role);
    }

    [Fact]
    public async Task GetMyProfile_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }


    [Fact]
    public async Task Participant_CannotCreateEvent_ReturnsForbidden()
    {
        var email = $"participant-{Guid.NewGuid():N}@example.com";
        const string password = "RaceDayTest2026!";

        var registration = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = email,
            Password = password,
            Role = "Participant"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register", registration);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest
            {
                Email = email,
                Password = password
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var eventRequest = new
        {
            eventName = "Test Race",
            description = "Authorization test",
            eventDate = DateTime.UtcNow.AddDays(30),
            locationId = 1,
            distanceKm = 5.0,
            eventType = "Run",
            status = "Upcoming"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Events", eventRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


}
