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
public class StreetsController : ControllerBase
{
    private readonly IStreetRepository _repo;
    private readonly ICurrentUser _current;
    private readonly IAuditService _audit;

    public StreetsController(IStreetRepository repo, ICurrentUser current, IAuditService audit)
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
    public async Task<IActionResult> Create([FromBody] Street street)
    {
        if (string.IsNullOrWhiteSpace(street.Name))
            return BadRequest(new { message = "Вкажіть назву вулиці." });

        if (_current.CommunityId is not > 0)
            return BadRequest(new { message = "Оберіть громаду." });

        street.CommunityId = _current.CommunityId;   // з токена, не з запиту
        street.Id = await _repo.CreateAsync(street);

        await _audit.LogAsync("streets", street.Id, "CREATE", null, street);
        return CreatedAtAction(nameof(GetById), new { id = street.Id }, street);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Street street)
    {
        var old = await _repo.GetByIdAsync(id);          // чужа вулиця: 404
        if (old == null) return NotFound();

        if (string.IsNullOrWhiteSpace(street.Name))
            return BadRequest(new { message = "Вкажіть назву вулиці." });

        street.Id = id;
        if (!await _repo.UpdateAsync(street)) return NotFound();

        await _audit.LogAsync("streets", id, "UPDATE", old, new { street.Id, street.Name });
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
            return Conflict(new { message = "Неможливо видалити: вулиця прив'язана до населених пунктів." });
        }

        await _audit.LogAsync("streets", id, "DELETE", old, null);
        return NoContent();
    }
}