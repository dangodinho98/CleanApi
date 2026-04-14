using System.Security.Claims;
using CleanApi.Application.Services;
using CleanApi.Domain.Entities;
using CleanApi.Web.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Web.Controllers;

/// <summary>JWT-protected JSON API for item CRUD.</summary>
[ApiController]
[Route("api/items")]
[Authorize]
public sealed class ItemsController(ItemService items) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ItemResponse>>> List(CancellationToken cancellationToken)
    {
        var rows = await items.ListAsync(cancellationToken);
        return Ok(rows.Select(Map).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var item = await items.GetByIdAsync(id, cancellationToken);
        if (item is null) return NotFound();

        return Ok(Map(item));
    }

    [HttpPost]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] CreateItemRequest body, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = GetUserIdOrThrow();
            var created = await items.CreateAsync(body.Title, body.Description, ownerId, cancellationToken);
            var response = Map(created);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemRequest body, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await items.UpdateAsync(id, body.Title, body.Description, cancellationToken);
            if (!ok) return NotFound();

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ok = await items.DeleteAsync(id, cancellationToken);
        if (!ok) return NotFound();

        return NoContent();
    }

    private Guid GetUserIdOrThrow()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(sub) || !Guid.TryParse(sub, out var id))
            throw new InvalidOperationException("Authenticated user id claim is missing.");
        return id;
    }

    private static ItemResponse Map(Item item) => new(item.Id, item.Title, item.Description, item.CreatedAtUtc, item.OwnerUserId);
}
