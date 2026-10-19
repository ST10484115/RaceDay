
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;

namespace RaceDay.API.Tests;

public class AuthIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;

        _client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    private IServiceScope FactoryServices()
    {
        return _factory.Services.CreateScope();
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

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest
            {
                Email = email,
                Password = "WrongPassword2026!"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_AllowsProfileAccess()
    {
        var email = $"valid-login-{Guid.NewGuid():N}@example.com";
        const string password = "RaceDayTest2026!";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Login",
                LastName = "Tester",
                Email = email,
                Password = password,
                Role = "Participant"
            });

        Assert.Equal(
            HttpStatusCode.Created, registerResponse.StatusCode);

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

        Assert.Equal(
            HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent_ReturnsForbidden()
    {
        var email = $"participant-{Guid.NewGuid():N}@example.com";
        const string password = "RaceDayTest2026!";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = email,
                Password = password,
                Role = "Participant"
            });

        Assert.Equal(
            HttpStatusCode.Created, registerResponse.StatusCode);

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

        Assert.Equal(
            HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanCreateEvent_WhenLocationExists()
    {
        var email = $"organiser-{Guid.NewGuid():N}@example.com";
        const string password = "RaceDayTest2026!";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = email,
                Password = password,
                Role = "Organiser"
            });

        Assert.Equal(
            HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest
            {
                Email = email,
                Password = password
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        int locationId;

        using (var scope = FactoryServices())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<RaceDayDbContext>();

            var location = new EventLocation
            {
                LocationName = "Test Stadium",
                Address = "1 Test Street",
                City = "Gqeberha",
                Province = "Eastern Cape"
            };

            db.EventLocations.Add(location);
            await db.SaveChangesAsync();

            locationId = location.LocationId;
        }

        var eventRequest = new
        {
            eventName = "Test Race",
            description = "Integration test event",
            eventDate = DateTime.UtcNow.AddDays(30),
            locationId,
            distanceKm = 5.0,
            eventType = "Run",
            status = "Upcoming"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Events", eventRequest);

        Assert.Equal(
            HttpStatusCode.Created, response.StatusCode);
    }
}
