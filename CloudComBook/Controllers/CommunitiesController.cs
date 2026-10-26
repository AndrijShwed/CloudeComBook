// Controllers/CommunitiesController.cs
using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommunitiesController : ControllerBase
{
    private readonly ICommunityRepository _repo;
    private readonly ICurrentUser _current;
    private readonly IAuditService _audit;

    public CommunitiesController(ICommunityRepository repo, ICurrentUser current, IAuditService audit)
    {
        _repo = repo;
        _current = current;
        _audit = audit;
    }

    [Authorize(Roles = "superadmin")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

    // назва власної громади (для адміна)
    [Authorize(Roles = "admin")]
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        if (_current.CommunityId /*is not int id or*/ <= 0) return NotFound();
        var c = await _repo.GetByIdAsync(_current.CommunityId.Value);
        return c == null ? NotFound() : Ok(c);
    }

    [Authorize(Roles = "superadmin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Community community)
    {
        if (string.IsNullOrWhiteSpace(community.Name))
            return BadRequest(new { message = "Вкажіть назву громади." });

        community.Id = await _repo.CreateAsync(community);
        await _audit.LogAsync("communities", community.Id, "CREATE", null, community);
        return Ok(community);
    }

    [Authorize(Roles = "superadmin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Community community)
    {
        var old = await _repo.GetByIdAsync(id);
        if (old == null) return NotFound();

        community.Id = id;
        if (!await _repo.UpdateAsync(community)) return NotFound();
        await _audit.LogAsync("communities", id, "UPDATE", old, community);
        return NoContent();
    }
}
