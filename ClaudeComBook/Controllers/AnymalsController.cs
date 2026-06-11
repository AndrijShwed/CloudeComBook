using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q) =>
        Ok(await _repo.SearchAsync(q));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Anymal anymal)
    {
        anymal.AnymalsId = await _repo.CreateAsync(anymal);
        return CreatedAtAction(nameof(GetById), new { id = anymal.AnymalsId }, anymal);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Anymal anymal)
    {
        anymal.AnymalsId = id;
        var ok = await _repo.UpdateAsync(anymal);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
