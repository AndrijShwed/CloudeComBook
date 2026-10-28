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
public class VillagesController : ControllerBase
{
    private readonly IVillageRepository _repo;
    private readonly ICurrentUser _current;
    private readonly IAuditService _audit;

    public VillagesController(IVillageRepository repo, ICurrentUser current, IAuditService audit)
    {
        _repo = repo;
        _current = current;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Village village)
    {
        if (string.IsNullOrWhiteSpace(village.Name))
            return BadRequest(new { message = "Вкажіть назву населеного пункту." });

        if (_current.CommunityId is not > 0)
            return BadRequest(new { message = "Оберіть громаду." });

        village.CommunityId = _current.CommunityId;   // з токена, не з запиту
        village.Id = await _repo.CreateAsync(village);

        await _audit.LogAsync("villages", village.Id, "CREATE", null, village);
        return CreatedAtAction(nameof(GetById), new { id = village.Id }, village);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Village village)
    {
        var old = await _repo.GetByIdAsync(id);          // чуже село: 404
        if (old == null) return NotFound();

        if (string.IsNullOrWhiteSpace(village.Name))
            return BadRequest(new { message = "Вкажіть назву населеного пункту." });

        village.Id = id;
        if (!await _repo.UpdateAsync(village)) return NotFound();

        await _audit.LogAsync("villages", id, "UPDATE", old, new { village.Id, village.Name });
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
            return Conflict(new { message = "Неможливо видалити: до населеного пункту прив'язані вулиці." });
        }

        await _audit.LogAsync("villages", id, "DELETE", old, null);
        return NoContent();
    }
}