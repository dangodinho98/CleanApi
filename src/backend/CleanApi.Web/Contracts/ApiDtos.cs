using System.ComponentModel.DataAnnotations;
using CleanApi.Application.Services;

namespace CleanApi.Web.Contracts;

/// <summary>Item returned from list, get, and create responses.</summary>
public sealed record ItemResponse(Guid Id, string Title, string Description, DateTime CreatedAtUtc, Guid? OwnerUserId);

/// <summary>Payload for <c>POST /api/items</c>.</summary>
public sealed class CreateItemRequest
{
    [Required]
    [StringLength(ItemService.MaxTitleLength, MinimumLength = 1)]
    public required string Title { get; init; }

    [Required]
    [StringLength(ItemService.MaxDescriptionLength, MinimumLength = 1)]
    public required string Description { get; init; }
}

/// <summary>Payload for updating an item (<c>PUT /api/items/:id</c>).</summary>
public sealed class UpdateItemRequest
{
    [Required]
    [StringLength(ItemService.MaxTitleLength, MinimumLength = 1)]
    public required string Title { get; init; }

    [Required]
    [StringLength(ItemService.MaxDescriptionLength, MinimumLength = 1)]
    public required string Description { get; init; }
}

/// <summary>Payload for <c>POST /api/auth/register</c>.</summary>
public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256, MinimumLength = 3)]
    public required string Email { get; init; }

    [Required]
    [StringLength(256, MinimumLength = AuthService.MinPasswordLength)]
    public required string Password { get; init; }

    [Required]
    [StringLength(AuthService.MaxDisplayNameLength, MinimumLength = 1)]
    public required string DisplayName { get; init; }
}

/// <summary>Payload for <c>POST /api/auth/login</c>.</summary>
public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256, MinimumLength = 3)]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}

/// <summary>Bearer token plus embedded user summary from auth endpoints.</summary>
public sealed record AuthResponse(string Token, UserSummary User);

/// <summary>Subset of user fields exposed to clients.</summary>
public sealed record UserSummary(Guid Id, string Email, string DisplayName);
