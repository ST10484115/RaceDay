
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


    [Fact]
    public async Task Organiser_CanRetrieveTheirOwnEvents()
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

        var response = await _client.GetAsync("/api/Events/mine");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    [Fact]
    public async Task Participant_CannotAccessOrganiserEvents_ReturnsForbidden()
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

        var response = await _client.GetAsync("/api/Events/mine");

        Assert.Equal(
            HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanCreateCategoryForTheirOwnEvent()
    {
        var email = $"category-organiser-{Guid.NewGuid():N}@example.com";
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
                LocationName = "Category Test Stadium",
                Address = "1 Test Street",
                City = "Gqeberha",
                Province = "Eastern Cape"
            };

            db.EventLocations.Add(location);
            await db.SaveChangesAsync();

            locationId = location.LocationId;
        }

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                eventName = "Category Test Race",
                description = "Event for category testing",
                eventDate = DateTime.UtcNow.AddDays(30),
                locationId,
                distanceKm = 10.0,
                eventType = "Run",
                status = "Upcoming"
            });

        Assert.Equal(
            HttpStatusCode.Created, eventResponse.StatusCode);

        var eventBody = await eventResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var eventId = eventBody.GetProperty("eventId").GetInt32();

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/Categories/event/{eventId}",
            new
            {
                categoryName = "10K",
                distanceKm = 10.0,
                entryFee = 100.00
            });

        Assert.Equal(
            HttpStatusCode.Created, categoryResponse.StatusCode);
    }


    [Fact]
    public async Task Participant_CanEnrolInUpcomingEvent()
    {
        var organiserEmail = $"enrol-organiser-{Guid.NewGuid():N}@example.com";
        var participantEmail = $"enrol-participant-{Guid.NewGuid():N}@example.com";
        const string password = "RaceDayTest2026!";

        var organiserRegistration = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = organiserEmail,
                Password = password,
                Role = "Organiser"
            });

        Assert.Equal(HttpStatusCode.Created, organiserRegistration.StatusCode);

        var organiserLogin = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest { Email = organiserEmail, Password = password });

        Assert.Equal(HttpStatusCode.OK, organiserLogin.StatusCode);

        int locationId;

        using (var scope = FactoryServices())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
            var location = new EventLocation
            {
                LocationName = "Enrolment Test Stadium",
                Address = "1 Test Street",
                City = "Gqeberha",
                Province = "Eastern Cape"
            };

            db.EventLocations.Add(location);
            await db.SaveChangesAsync();
            locationId = location.LocationId;
        }

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                eventName = "Enrolment Test Race",
                description = "Upcoming event for enrolment testing",
                eventDate = DateTime.UtcNow.AddDays(30),
                locationId,
                distanceKm = 10.0,
                eventType = "Run",
                status = "Upcoming"
            });

        Assert.Equal(HttpStatusCode.Created, eventResponse.StatusCode);

        var eventBody = await eventResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var eventId = eventBody.GetProperty("eventId").GetInt32();

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/Categories/event/{eventId}",
            new
            {
                categoryName = "10K",
                distanceKm = 10.0,
                entryFee = 100.00
            });

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryBody = await categoryResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var categoryId = categoryBody.GetProperty("categoryId").GetInt32();

        // Log out the organiser before registering and logging in as a participant.
        var logoutResponse = await _client.PostAsync("/api/Auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var participantRegistration = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = participantEmail,
                Password = password,
                Role = "Participant"
            });

        Assert.Equal(HttpStatusCode.Created, participantRegistration.StatusCode);

        var participantLogin = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest { Email = participantEmail, Password = password });

        Assert.Equal(HttpStatusCode.OK, participantLogin.StatusCode);

        var enrolmentResponse = await _client.PostAsJsonAsync(
            "/api/Enrolments",
            new
            {
                eventId,
                categoryId
            });

        Assert.Equal(HttpStatusCode.Created, enrolmentResponse.StatusCode);
    }


    [Fact]
    public async Task Participant_CannotEnrolInSameCategoryTwice()
    {
        var organiserEmail = $"duplicate-organiser-{Guid.NewGuid():N}@example.com";
        var participantEmail = $"duplicate-participant-{Guid.NewGuid():N}@example.com";
        const string password = "RaceDayTest2026!";

        var organiserRegistration = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = organiserEmail,
                Password = password,
                Role = "Organiser"
            });

        Assert.Equal(HttpStatusCode.Created, organiserRegistration.StatusCode);

        var organiserLogin = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest { Email = organiserEmail, Password = password });

        Assert.Equal(HttpStatusCode.OK, organiserLogin.StatusCode);

        int locationId;

        using (var scope = FactoryServices())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();

            var location = new EventLocation
            {
                LocationName = "Duplicate Test Stadium",
                Address = "1 Test Street",
                City = "Gqeberha",
                Province = "Eastern Cape"
            };

            db.EventLocations.Add(location);
            await db.SaveChangesAsync();
            locationId = location.LocationId;
        }

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                eventName = "Duplicate Enrolment Race",
                description = "Duplicate enrolment test",
                eventDate = DateTime.UtcNow.AddDays(30),
                locationId,
                distanceKm = 10.0,
                eventType = "Run",
                status = "Upcoming"
            });

        Assert.Equal(HttpStatusCode.Created, eventResponse.StatusCode);

        var eventBody = await eventResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var eventId = eventBody.GetProperty("eventId").GetInt32();

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/Categories/event/{eventId}",
            new
            {
                categoryName = "10K",
                distanceKm = 10.0,
                entryFee = 100.00
            });

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryBody = await categoryResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var categoryId = categoryBody.GetProperty("categoryId").GetInt32();

        var logoutResponse = await _client.PostAsync("/api/Auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var participantRegistration = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = participantEmail,
                Password = password,
                Role = "Participant"
            });

        Assert.Equal(HttpStatusCode.Created, participantRegistration.StatusCode);

        var participantLogin = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest { Email = participantEmail, Password = password });

        Assert.Equal(HttpStatusCode.OK, participantLogin.StatusCode);

        var firstEnrolment = await _client.PostAsJsonAsync(
            "/api/Enrolments",
            new { eventId, categoryId });

        Assert.Equal(HttpStatusCode.Created, firstEnrolment.StatusCode);

        var duplicateEnrolment = await _client.PostAsJsonAsync(
            "/api/Enrolments",
            new { eventId, categoryId });

        Assert.Equal(HttpStatusCode.Conflict, duplicateEnrolment.StatusCode);
    }



    [Fact]
    public async Task Organiser_CanRecordResultForTheirEventEnrolment()
    {
        var organiserEmail = $"result-organiser-{Guid.NewGuid():N}@example.com";
        var participantEmail = $"result-participant-{Guid.NewGuid():N}@example.com";
        const string password = "RaceDayTest2026!";

        var organiserRegistration = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Organiser",
                Email = organiserEmail,
                Password = password,
                Role = "Organiser"
            });

        Assert.Equal(HttpStatusCode.Created, organiserRegistration.StatusCode);

        var organiserLogin = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest { Email = organiserEmail, Password = password });

        Assert.Equal(HttpStatusCode.OK, organiserLogin.StatusCode);

        int locationId;

        using (var scope = FactoryServices())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();

            var location = new EventLocation
            {
                LocationName = "Results Test Stadium",
                Address = "1 Test Street",
                City = "Gqeberha",
                Province = "Eastern Cape"
            };

            db.EventLocations.Add(location);
            await db.SaveChangesAsync();
            locationId = location.LocationId;
        }

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                eventName = "Results Test Race",
                description = "Result recording test",
                eventDate = DateTime.UtcNow.AddDays(30),
                locationId,
                distanceKm = 10.0,
                eventType = "Run",
                status = "Upcoming"
            });

        Assert.Equal(HttpStatusCode.Created, eventResponse.StatusCode);

        var eventBody = await eventResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var eventId = eventBody.GetProperty("eventId").GetInt32();

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/Categories/event/{eventId}",
            new
            {
                categoryName = "10K",
                distanceKm = 10.0,
                entryFee = 100.00
            });

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryBody = await categoryResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var categoryId = categoryBody.GetProperty("categoryId").GetInt32();

        var logoutResponse = await _client.PostAsync("/api/Auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var participantRegistration = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Participant",
                Email = participantEmail,
                Password = password,
                Role = "Participant"
            });

        Assert.Equal(HttpStatusCode.Created, participantRegistration.StatusCode);

        var participantLogin = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest { Email = participantEmail, Password = password });

        Assert.Equal(HttpStatusCode.OK, participantLogin.StatusCode);

        var enrolmentResponse = await _client.PostAsJsonAsync(
            "/api/Enrolments",
            new { eventId, categoryId });

        Assert.Equal(HttpStatusCode.Created, enrolmentResponse.StatusCode);

        var enrolmentBody = await enrolmentResponse.Content
            .ReadFromJsonAsync<System.Text.Json.JsonElement>();

        var enrolmentId = enrolmentBody.GetProperty("enrolmentId").GetInt32();

        var participantLogout = await _client.PostAsync("/api/Auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, participantLogout.StatusCode);

        var organiserLoginAgain = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest { Email = organiserEmail, Password = password });

        Assert.Equal(HttpStatusCode.OK, organiserLoginAgain.StatusCode);

        var resultResponse = await _client.PostAsJsonAsync(
            $"/api/Results/{enrolmentId}",
            new
            {
                finishTime = "00:55:30",
                position = 1,
                resultStatus = "Recorded"
            });

        Assert.Equal(HttpStatusCode.Created, resultResponse.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotRecordResults_ReturnsForbidden()
    {
        var email = $"result-participant-{Guid.NewGuid():N}@example.com";
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

        var response = await _client.PostAsJsonAsync(
            "/api/Results/999",
            new
            {
                finishTime = "00:55:30",
                position = 1,
                resultStatus = "Recorded"
            });

        Assert.Equal(
            HttpStatusCode.Forbidden, response.StatusCode);
    }


    [Fact]
    public async Task ResultsEndpoint_RequiresAuthentication()
    {
        // Log out in case the shared test client has an active session.
        await _client.PostAsync("/api/Auth/logout", null);

        var response = await _client.GetAsync("/api/Results");

        Assert.Equal(
            HttpStatusCode.Unauthorized, response.StatusCode);
    }


}
