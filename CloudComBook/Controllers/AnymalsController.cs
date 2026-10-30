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
public class AnymalsController : ControllerBase
{
    private readonly IAnymalRepository _repo;
    private readonly ICurrentUser _current;
    private readonly ICommunityGuard _guard;
    private readonly IAuditService _audit;

    public AnymalsController(IAnymalRepository repo, ICurrentUser current, ICommunityGuard guard, IAuditService audit)
    {
        _repo = repo;
        _current = current;
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

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(
        [FromQuery] string lastName,
        [FromQuery] string name,
        [FromQuery] string village,
        [FromQuery] string? surname = null) =>
        Ok(await _repo.ExistsAsync(lastName, name, surname, village));

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? lastName = null,
        [FromQuery] string? name = null,
        [FromQuery] string? surname = null,
        [FromQuery] string? village = null,
        [FromQuery] bool hasCovs = false,
        [FromQuery] bool hasHorses = false,
        [FromQuery] bool hasPigs = false,
        [FromQuery] bool hasSheeps = false,
        [FromQuery] bool hasGoats = false,
        [FromQuery] bool hasBirds = false,
        [FromQuery] bool hasRabbits = false,
        [FromQuery] bool hasBeeses = false)
    {
        var result = await _repo.SearchAsync(
            lastName, name, surname, village,
            hasCovs, hasHorses, hasPigs, hasSheeps,
            hasGoats, hasBirds, hasRabbits, hasBeeses);
        return Ok(result);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics() =>
        Ok(await _repo.GetStatisticsByVillageAsync());

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Anymal anymal)
    {
        if (_current.CommunityId is not > 0)
            return BadRequest(new { message = "Оберіть громаду." });

        var communityId = _current.CommunityId.Value;
        if (!await _guard.VillageNameInCommunityAsync(anymal.Village, communityId))
            return BadRequest(new { message = "Населений пункт не належить вашій громаді." });

        anymal.CommunityId = communityId;   // з токена, не з запиту
        anymal.AnymalsId = await _repo.CreateAsync(anymal);

        await _audit.LogAsync("anymals", anymal.AnymalsId, "CREATE", null, anymal);
        return CreatedAtAction(nameof(GetById), new { id = anymal.AnymalsId }, anymal);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Anymal anymal)
    {
        var old = await _repo.GetByIdAsync(id);              // чужий запис: 404
        if (old == null) return NotFound();

        if (!await _guard.VillageNameInCommunityAsync(anymal.Village, old.CommunityId ?? 0))
            return BadRequest(new { message = "Населений пункт не належить громаді цього запису." });

        anymal.AnymalsId = id;
        anymal.CommunityId = old.CommunityId;
        if (!await _repo.UpdateAsync(anymal)) return NotFound();

        await _audit.LogAsync("anymals", id, "UPDATE", old, anymal);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var old = await _repo.GetByIdAsync(id);
        if (old == null) return NotFound();

        if (!await _repo.DeleteAsync(id)) return NotFound();

        await _audit.LogAsync("anymals", id, "DELETE", old, null);
        return NoContent();
    }
}