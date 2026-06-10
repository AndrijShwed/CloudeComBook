using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VillagesController : ControllerBase
{
    private readonly IVillageRepository _repo;

    public VillagesController(IVillageRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Village village)
    {
        village.Id = await _repo.CreateAsync(village);
        return CreatedAtAction(nameof(GetById), new { id = village.Id }, village);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Village village)
    {
        village.Id = id;
        var ok = await _repo.UpdateAsync(village);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
