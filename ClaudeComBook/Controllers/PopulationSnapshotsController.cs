using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PopulationSnapshotsController : ControllerBase
{
    private readonly IPopulationSnapshotRepository _repo;

    public PopulationSnapshotsController(IPopulationSnapshotRepository repo) => _repo = repo;

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
    public async Task<IActionResult> Search([FromQuery] string q) =>
        Ok(await _repo.SearchAsync(q));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PopulationSnapshot populationSnapshot)
    {
        populationSnapshot.Id = await _repo.CreateAsync(populationSnapshot);
        return CreatedAtAction(nameof(GetById), new { id = populationSnapshot.Id }, populationSnapshot);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PopulationSnapshot populationSnapshot)
    {
        populationSnapshot.Id = id;
        var ok = await _repo.UpdateAsync(populationSnapshot);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] PopulationSnapshot snapshot)
    {
        var exists = await _repo.ExistsForYearAndVillageAsync(
            snapshot.Year ?? 0, snapshot.SettlementName ?? "");

        if (exists)
            await _repo.UpdateByYearAndVillageAsync(
                snapshot.Year ?? 0, snapshot.SettlementName ?? "", snapshot.Population ?? 0);
        else
            await _repo.CreateAsync(snapshot);

        return Ok();
    }
}
