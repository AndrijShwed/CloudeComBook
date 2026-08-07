using ClaudeComBook.API.Repositories.Interfaces;
using ClaudeComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "reader,user,admin")]
public class VillageStreetsController : ControllerBase
{
    private readonly IVillageStreetRepository _repo;

    public VillageStreetsController(IVillageStreetRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("by-village/{villageId}")]
    public async Task<IActionResult> GetByVillage(int villageId) =>
        Ok(await _repo.GetByVillageIdAsync(villageId));

    [HttpGet("by-street/{streetId}")]
    public async Task<IActionResult> GetByStreet(int streetId) =>
        Ok(await _repo.GetByStreetIdAsync(streetId));

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VillageStreet villageStreet)
    {
        villageStreet.Id = await _repo.CreateAsync(villageStreet);
        return CreatedAtAction(nameof(GetById), new { id = villageStreet.Id }, villageStreet);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] VillageStreet villageStreet)
    {
        villageStreet.Id = id;
        var ok = await _repo.UpdateAsync(villageStreet);
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