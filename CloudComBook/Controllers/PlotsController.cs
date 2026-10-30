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
public class PlotsController : ControllerBase
{
    private readonly IPlotRepository _repo;
    private readonly ICurrentUser _current;
    private readonly ICommunityGuard _guard;
    private readonly IAuditService _audit;

    public PlotsController(IPlotRepository repo, ICurrentUser current, ICommunityGuard guard, IAuditService audit)
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

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? fullName = null,
        [FromQuery] string? village = null,
        [FromQuery] string? street = null,
        [FromQuery] string? houseNumb = null,
        [FromQuery] string? fieldNumber = null,
        [FromQuery] string? plotType = null,
        [FromQuery] string? plotNumber = null,
        [FromQuery] string? tenant = null,
        [FromQuery] string? cadastr = null)
    {
        var result = await _repo.SearchAsync(
            fullName, village, street, houseNumb,
            fieldNumber, plotType, plotNumber, tenant, cadastr);
        return Ok(result);
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(
        [FromQuery] string? cadastr = null,
        [FromQuery] string? village = null,
        [FromQuery] string? street = null,
        [FromQuery] string? houseNumb = null,
        [FromQuery] string? plotType = null,
        [FromQuery] int? excludeId = null) =>
        Ok(await _repo.ExistsAsync(cadastr, village, street, houseNumb, plotType, excludeId));

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Plot plot)
    {
        if (_current.CommunityId is not > 0)
            return BadRequest(new { message = "Оберіть громаду." });

        var communityId = _current.CommunityId.Value;
        if (!await _guard.VillageNameInCommunityAsync(plot.Village, communityId))
            return BadRequest(new { message = "Населений пункт не належить вашій громаді." });

        plot.CommunityId = communityId;   // з токена, не з запиту
        plot.Id = await _repo.CreateAsync(plot);

        await _audit.LogAsync("plot", plot.Id, "CREATE", null, plot);
        return CreatedAtAction(nameof(GetById), new { id = plot.Id }, plot);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Plot plot)
    {
        var old = await _repo.GetByIdAsync(id);              // чужа ділянка: 404
        if (old == null) return NotFound();

        // село має лишатися в громаді самої ділянки
        if (!await _guard.VillageNameInCommunityAsync(plot.Village, old.CommunityId ?? 0))
            return BadRequest(new { message = "Населений пункт не належить громаді цієї ділянки." });

        plot.Id = id;
        plot.CommunityId = old.CommunityId;
        if (!await _repo.UpdateAsync(plot)) return NotFound();

        await _audit.LogAsync("plot", id, "UPDATE", old, plot);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var old = await _repo.GetByIdAsync(id);
        if (old == null) return NotFound();

        if (!await _repo.DeleteAsync(id)) return NotFound();

        await _audit.LogAsync("plot", id, "DELETE", old, null);
        return NoContent();
    }
}