using CloudeComBook.API.Repositories.Interfaces;
using CloudeComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "reader,user,admin")]
public class AnymalsController : ControllerBase
{
    private readonly IAnymalRepository _repo;

    public AnymalsController(IAnymalRepository repo) => _repo = repo;

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
    [FromQuery] string? surname = null)
    {
        var exists = await _repo.ExistsAsync(lastName, name, surname, village);
        return Ok(exists);
    }

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
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _repo.GetStatisticsByVillageAsync();
        return Ok(result);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Anymal anymal)
    {
        anymal.AnymalsId = await _repo.CreateAsync(anymal);
        return CreatedAtAction(nameof(GetById), new { id = anymal.AnymalsId }, anymal);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Anymal anymal)
    {
        anymal.AnymalsId = id;
        var ok = await _repo.UpdateAsync(anymal);
        return ok ? NoContent() : NotFound();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    
}
