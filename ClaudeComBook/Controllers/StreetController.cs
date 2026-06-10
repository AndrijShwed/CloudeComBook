using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreetsController : ControllerBase
{
    private readonly IStreetRepository _repo;

    public StreetsController(IStreetRepository repo) => _repo = repo;

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
    public async Task<IActionResult> Create([FromBody] Street street)
    {
        street.Id = await _repo.CreateAsync(street);
        return CreatedAtAction(nameof(GetById), new { id = street.Id }, street);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Street street)
    {
        street.Id = id;
        var ok = await _repo.UpdateAsync(street);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}

