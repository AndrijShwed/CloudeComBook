using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace CloudComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "reader,user,admin")]
public class VillageStreetsController : ControllerBase
{
    private readonly IVillageStreetRepository _repo;
    private readonly ICommunityGuard _guard;
    private readonly IAuditService _audit;

    public VillageStreetsController(IVillageStreetRepository repo, ICommunityGuard guard, IAuditService audit)
    {
        _repo = repo;
        _guard = guard;
        _audit = audit;
    }

    // у журнал без файлу (він може бути великим)
    private static object Safe(VillageStreet v) => new
    {
        v.Id,
        v.VillageId,
        v.StreetId,
        v.OldStreetId,
        v.IsActive,
        v.RenameDate,
        v.FileData
    };

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
        if (!await LinksAllowedAsync(villageStreet))
            return Forbid();

        try
        {
            villageStreet.Id = await _repo.CreateAsync(villageStreet);
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Conflict(new { message = "Така вулиця вже є в цьому населеному пункті." });
        }

        await _audit.LogAsync("villagestreet", villageStreet.Id, "CREATE", null, Safe(villageStreet));
        return CreatedAtAction(nameof(GetById), new { id = villageStreet.Id }, villageStreet);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] VillageStreet villageStreet)
    {
        var old = await _repo.GetByIdAsync(id);              // чужий запис: 404
        if (old == null) return NotFound();

        if (!await LinksAllowedAsync(villageStreet))
            return Forbid();

        villageStreet.Id = id;
        try
        {
            if (!await _repo.UpdateAsync(villageStreet)) return NotFound();
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            return Conflict(new { message = "Така вулиця вже є в цьому населеному пункті." });
        }

        await _audit.LogAsync("villagestreet", id, "UPDATE", Safe(old), Safe(villageStreet));
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
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.RowIsReferenced)
        {
            return Conflict(new { message = "Неможливо видалити: до цієї вулиці прив'язані мешканці, будинки або підприємства." });
        }

        await _audit.LogAsync("villagestreet", id, "DELETE", Safe(old), null);
        return NoContent();
    }
    private async Task<bool> LinksAllowedAsync(VillageStreet v)
    {
        if (!await _guard.VillageAllowedAsync(v.VillageId)) return false;
        if (!await _guard.StreetAllowedAsync(v.StreetId)) return false;
        if (!await _guard.SameCommunityAsync(v.VillageId, v.StreetId)) return false;

        int? oldStreetId = v.OldStreetId;
        if (oldStreetId is > 0 && !await _guard.StreetAllowedAsync(oldStreetId.Value))
            return false;
        return true;
    }
}