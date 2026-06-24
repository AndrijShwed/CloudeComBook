using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ClaudeComBook.API.DTOs;
using static ClaudeComBook.API.DTOs.DTOs;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VillageStreet villageStreet)
    {
        villageStreet.Id = await _repo.CreateAsync(villageStreet);
        return CreatedAtAction(nameof(GetById), new { id = villageStreet.Id }, villageStreet);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] VillageStreet villageStreet)
    {
        villageStreet.Id = id;
        var ok = await _repo.UpdateAsync(villageStreet);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpPut("{id}/file")]
    public async Task<IActionResult> UpdateFile(int id, [FromBody] UpdateFileRequest request)
    {
        var fileData = Convert.FromBase64String(request.FileData);
        var ok = await _repo.UpdateFileAsync(id, fileData);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("rename")]
    public async Task<IActionResult> Rename([FromBody] RenameStreetRequest request)
    {
        var ok = await _repo.RenameStreetAsync(
            request.VillageId,
            request.OldStreetId,
            request.NewStreetId,
            request.RenameDate,
            request.FileData != null ? Convert.FromBase64String(request.FileData) : null);
        return ok ? NoContent() : BadRequest();
    }
}