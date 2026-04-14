using CleanApi.Application.Services;
using CleanApi.Web.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CleanApi.Web.Controllers;

/// <summary>Authenticated account endpoints (current user profile).</summary>
[ApiController]
[Route("api/account")]
[Authorize]
public sealed class AccountController(AuthService auth) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserSummary>> Me(CancellationToken cancellationToken)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var id))
            return Unauthorized();

        var user = await auth.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
            return Unauthorized();

        return Ok(new UserSummary(user.Id, user.Email, user.DisplayName));
    }
}
