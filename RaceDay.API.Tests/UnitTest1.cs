
using System.ComponentModel.DataAnnotations;
using RaceDay.API.DTOs;

namespace RaceDay.API.Tests;

public class UnitTest1
{
    [Fact]
    public void RegisterRequest_WithValidDetails_PassesValidation()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@example.com",
            Password = "RaceDayTest2026!",
            Role = "Participant"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(
            request, context, results, validateAllProperties: true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void RegisterRequest_WithInvalidEmail_FailsValidation()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "not-an-email",
            Password = "RaceDayTest2026!",
            Role = "Participant"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(
            request, context, results, validateAllProperties: true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }


    [Fact]
    public void RegisterRequest_WithShortPassword_FailsValidation()
    {
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@example.com",
            Password = "short",
            Role = "Participant"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Fact]
    public void RegisterRequest_WithInvalidRole_FailsValidation()
    {
        var request = new RegisterRequest
        {
            FirstName = "Test",
            LastName = "Participant",
            Email = "participant@example.com",
            Password = "RaceDayTest2026!",
            Role = "Admin"
        };

        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(RegisterRequest.Role)));
    }

}

