using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HousesController : ControllerBase
{
    private readonly IHouseRepository _repo;

    public HousesController(IHouseRepository repo) => _repo = repo;

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
    [FromQuery] int? villageId = null,
    [FromQuery] int? streetId = null,
    [FromQuery] string? houseNumber = null,
    [FromQuery] string? lastName = null,
    [FromQuery] string? name = null,
    [FromQuery] string? surname = null)
    {
        var result = await _repo.SearchAsync(
            villageId, streetId, houseNumber, lastName, name, surname);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] House house)
    {
        house.IdHouses = await _repo.CreateAsync(house);
        return CreatedAtAction(nameof(GetById), new { id = house.IdHouses }, house);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] House house)
    {
        house.IdHouses = id;
        var ok = await _repo.UpdateAsync(house);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(
    [FromQuery] int villageStreetId,
    [FromQuery] string numbOfHouse)
    {
        var exists = await _repo.ExistsAsync(villageStreetId, numbOfHouse);
        return Ok(exists);
    }
}
