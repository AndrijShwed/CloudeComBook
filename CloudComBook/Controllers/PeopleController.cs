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
public class PeopleController : ControllerBase
{
    private readonly IPersonRepository _repo;
    private readonly ICommunityGuard _guard;
    private readonly IAuditService _audit;

    public PeopleController(IPersonRepository repo, ICommunityGuard guard, IAuditService audit)
    {
        _repo = repo;
        _guard = guard;
        _audit = audit;
    }

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

    [HttpGet("population-by-village")]
    public async Task<IActionResult> GetPopulationByVillage() =>
        Ok(await _repo.GetPopulationByVillageAsync());

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(
        [FromQuery] string lastName,
        [FromQuery] string name,
        [FromQuery] string? surname = null,
        [FromQuery] DateTime? dateOfBirth = null) =>
        Ok(await _repo.ExistsAsync(lastName, name, surname, dateOfBirth));

    [HttpGet("by-address")]
    public async Task<IActionResult> GetByAddress(
        [FromQuery] int villageStreetId,
        [FromQuery] string numbOfHouse,
        [FromQuery] int? id) =>
        Ok(await _repo.GetByAddressAsync(villageStreetId, numbOfHouse, id));

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Person person)
    {
        if (!await _guard.VillageStreetAllowedAsync(person.VillageStreetId))
            return Forbid();

        person.PeopleId = await _repo.CreateAsync(person);
        await _audit.LogAsync("people", person.PeopleId, "CREATE", null, person);
        return CreatedAtAction(nameof(GetById), new { id = person.PeopleId }, person);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Person person)
    {
        var old = await _repo.GetByIdAsync(id);          // чужий запис: 404
        if (old == null) return NotFound();

        if (!await _guard.VillageStreetAllowedAsync(person.VillageStreetId))
            return Forbid();                              // не можна перенести в чужу громаду

        person.PeopleId = id;
        if (!await _repo.UpdateAsync(person)) return NotFound();

        await _audit.LogAsync("people", id, "UPDATE", old, person);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var old = await _repo.GetByIdAsync(id);
        if (old == null) return NotFound();

        if (!await _repo.DeleteAsync(id)) return NotFound();

        await _audit.LogAsync("people", id, "DELETE", old, null);
        return NoContent();
    }
}