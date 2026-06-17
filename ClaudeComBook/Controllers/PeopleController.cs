using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly IPersonRepository _repo;

    public PeopleController(IPersonRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] string? lastName = null,
    [FromQuery] string? name = null,
    [FromQuery] string? surname = null,
    [FromQuery] string? sex = null,
    [FromQuery] string? status = null,
    [FromQuery] string? registr = null,
    [FromQuery] int? villageId = null,
    [FromQuery] int? streetId = null,
    [FromQuery] string? houseNumb = null,
    [FromQuery] int? ageFrom = null,
    [FromQuery] int? ageTo = null,
    [FromQuery] int? statusYear = null)
    {
        var result = await _repo.GetAllAsync(
            lastName, name, surname, sex, status,
            registr, villageId, streetId, houseNumb, ageFrom, ageTo, statusYear);
        return Ok(result);
    }

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
    public async Task<IActionResult> Search([FromQuery] string q) =>
        Ok(await _repo.SearchAsync(q));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Person person)
    {
        person.PeopleId = await _repo.CreateAsync(person);
        return CreatedAtAction(nameof(GetById), new { id = person.PeopleId }, person);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Person person)
    {
        person.PeopleId = id;
        var ok = await _repo.UpdateAsync(person);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
