using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "reader,user,admin")]
public class EnterprisesController : ControllerBase
{
    private readonly IEnterpriseRepository _repo;
    private readonly ICommunityGuard _guard;
    private readonly IAuditService _audit;

    public EnterprisesController(IEnterpriseRepository repo, ICommunityGuard guard, IAuditService audit)
    {
        _repo = repo;
        _guard = guard;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("by-villagestreet/{villageStreetId}")]
    public async Task<IActionResult> GetByVillageStreet(int villageStreetId) =>
        Ok(await _repo.GetByVillageStreetIdAsync(villageStreetId));

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? name = null,
        [FromQuery] string? owner = null,
        [FromQuery] int? villageId = null,
        [FromQuery] int? streetId = null,
        [FromQuery] string? houseNumber = null) =>
        Ok(await _repo.SearchAsync(name, owner, villageId, streetId, houseNumber));

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(
        [FromQuery] string name,
        [FromQuery] int? villageStreetId = null,
        [FromQuery] string? houseNumber = null,
        [FromQuery] int? excludeId = null) =>
        Ok(await _repo.ExistsAsync(name, villageStreetId, houseNumber, excludeId));

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Enterprise enterprise)
    {
        if (!await _guard.VillageStreetAllowedAsync(enterprise.VillageStreetId))
            return Forbid();

        enterprise.Id = await _repo.CreateAsync(enterprise);
        await _audit.LogAsync("enterprises", enterprise.Id, "CREATE", null, enterprise);
        return CreatedAtAction(nameof(GetById), new { id = enterprise.Id }, enterprise);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Enterprise enterprise)
    {
        var old = await _repo.GetByIdAsync(id);               // чуже підприємство: 404
        if (old == null) return NotFound();

        if (!await _guard.VillageStreetAllowedAsync(enterprise.VillageStreetId))
            return Forbid();                                   // не можна перенести в чужу громаду

        enterprise.Id = id;
        if (!await _repo.UpdateAsync(enterprise)) return NotFound();

        await _audit.LogAsync("enterprises", id, "UPDATE", old, enterprise);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var old = await _repo.GetByIdAsync(id);
        if (old == null) return NotFound();

        if (!await _repo.DeleteAsync(id)) return NotFound();

        await _audit.LogAsync("enterprises", id, "DELETE", old, null);
        return NoContent();
    }
}