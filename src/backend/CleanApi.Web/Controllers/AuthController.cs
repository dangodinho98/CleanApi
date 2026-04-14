using CleanApi.Application.Services;
using CleanApi.Web.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Web.Controllers;

/// <summary>Anonymous registration and login returning JWT + user summary.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest body, CancellationToken cancellationToken)
    {
        try
        {
            var (user, token) = await auth.RegisterAsync(body.Email, body.Password, body.DisplayName, cancellationToken);
            return Ok(new AuthResponse(token, new UserSummary(user.Id, user.Email, user.DisplayName)));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest body, CancellationToken cancellationToken)
    {
        try
        {
            var (user, token) = await auth.LoginAsync(body.Email, body.Password, cancellationToken);
            return Ok(new AuthResponse(token, new UserSummary(user.Id, user.Email, user.DisplayName)));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized();
        }
    }
}
