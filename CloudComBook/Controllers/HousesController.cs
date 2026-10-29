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
public class HousesController : ControllerBase
{
    private readonly IHouseRepository _repo;
    private readonly ICommunityGuard _guard;
    private readonly IAuditService _audit;

    public HousesController(IHouseRepository repo, ICommunityGuard guard, IAuditService audit)
    {
        _repo = repo;
        _guard = guard;
        _audit = audit;
    }

    [HttpGet("by-village/{villageId}")]
    public async Task<IActionResult> GetByVillage(int villageId) =>
        Ok(await _repo.GetByVillageIdAsync(villageId));

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

    [HttpGet("area-by-village")]
    public async Task<IActionResult> GetAreaByVillage()
    {
        var result = await _repo.GetAreaByVillageAsync();
        return Ok(result.Select(r => new { r.Village, r.TotalArea, r.LivingArea }));
    }

    [HttpGet("rooms-by-village")]
    public async Task<IActionResult> GetRoomsByVillage() =>
        Ok(await _repo.GetRoomCountByVillageAsync());

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(
        [FromQuery] int villageStreetId,
        [FromQuery] string numbOfHouse) =>
        Ok(await _repo.ExistsAsync(villageStreetId, numbOfHouse));

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] House house)
    {
        if (!await _guard.VillageStreetAllowedAsync(house.VillageStreetId))
            return Forbid();

        house.IdHouses = await _repo.CreateAsync(house);
        await _audit.LogAsync("houses", house.IdHouses, "CREATE", null, house);
        return CreatedAtAction(nameof(GetById), new { id = house.IdHouses }, house);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] House house)
    {
        var old = await _repo.GetByIdAsync(id);              // чужий будинок: 404
        if (old == null) return NotFound();

        if (!await _guard.VillageStreetAllowedAsync(house.VillageStreetId))
            return Forbid();                                  // не можна перенести в чужу громаду

        house.IdHouses = id;
        if (!await _repo.UpdateAsync(house)) return NotFound();

        await _audit.LogAsync("houses", id, "UPDATE", old, house);
        return NoContent();
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var old = await _repo.GetByIdAsync(id);
        if (old == null) return NotFound();

        try
        {
            if (!await _repo.DeleteAsync(id)) return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        await _audit.LogAsync("houses", id, "DELETE", old, null);
        return NoContent();
    }
}