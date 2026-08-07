using ClaudeComBook.API.Repositories.Interfaces;
using ClaudeComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "reader,user,admin")]
public class EnterprisesController: ControllerBase
{
    private readonly IEnterpriseRepository _repo;

    public EnterprisesController(IEnterpriseRepository repo) => _repo = repo;

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
    [FromQuery] string? houseNumber = null)
    {
        var result = await _repo.SearchAsync(name, owner, villageId, streetId, houseNumber);
        return Ok(result);
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists([FromQuery] string name)
    {
        var exists = await _repo.ExistsByNameAsync(name);
        return Ok(exists);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Enterprise enterprise)
    {
        enterprise.Id = await _repo.CreateAsync(enterprise);
        return CreatedAtAction(nameof(GetById), new { id = enterprise.Id }, enterprise);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Enterprise enterprise)
    {
        enterprise.Id = id;
        var ok = await _repo.UpdateAsync(enterprise);
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

