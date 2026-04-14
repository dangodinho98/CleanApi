using System.ComponentModel.DataAnnotations;
using CleanApi.Web.Contracts;

namespace CleanApi.Web.Tests.UnitTests;

public sealed class ApiContractsValidationTests
{
    private static void AssertInvalid(object request)
    {
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);
        Assert.False(valid);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void RegisterRequest_invalid_when_password_too_short() =>
        AssertInvalid(new RegisterRequest { Email = "a@test.com", Password = "short", DisplayName = "Name" });

    [Fact]
    public void RegisterRequest_invalid_when_email_too_short() =>
        AssertInvalid(new RegisterRequest { Email = "ab", Password = "password12", DisplayName = "Name" });

    [Fact]
    public void RegisterRequest_invalid_when_display_name_empty() =>
        AssertInvalid(new RegisterRequest { Email = "a@test.com", Password = "password12", DisplayName = "" });

    [Fact]
    public void LoginRequest_invalid_when_email_not_valid_address() =>
        AssertInvalid(new LoginRequest { Email = "not-an-email", Password = "password12" });

    [Fact]
    public void CreateItemRequest_invalid_when_title_empty() =>
        AssertInvalid(new CreateItemRequest { Title = "", Description = "desc" });

    [Fact]
    public void CreateItemRequest_invalid_when_description_empty() =>
        AssertInvalid(new CreateItemRequest { Title = "t", Description = "" });
}
