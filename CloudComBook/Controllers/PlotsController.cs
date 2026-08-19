using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "reader,user,admin")]

public class PlotsController : ControllerBase
{
    private readonly IPlotRepository _repo;

    public PlotsController(IPlotRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? NotFound() : Ok(item);
    }


    [HttpGet("search")]
    public async Task<IActionResult> Search(
    [FromQuery] string? fullName = null,
    [FromQuery] string? village = null,
    [FromQuery] string? street = null,
    [FromQuery] string? houseNumb = null,
    [FromQuery] string? fieldNumber = null,
    [FromQuery] string? plotType = null,
    [FromQuery] string? plotNumber = null,
    [FromQuery] string? tenant = null,
    [FromQuery] string? cadastr = null)
    {
        var result = await _repo.SearchAsync(
            fullName, village, street, houseNumb,
            fieldNumber, plotType, plotNumber, tenant, cadastr);
        return Ok(result);
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(
    [FromQuery] string? cadastr = null,
    [FromQuery] string? village = null,
    [FromQuery] string? street = null,
    [FromQuery] string? houseNumb = null,
    [FromQuery] string? plotType = null,
    [FromQuery] int? excludeId = null)
    {
        var exists = await _repo.ExistsAsync(cadastr, village, street, houseNumb, plotType, excludeId);
        return Ok(exists);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Plot plot)
    {
        plot.Id = await _repo.CreateAsync(plot);
        return CreatedAtAction(nameof(GetById), new { id = plot.Id }, plot);
    }

    [Authorize(Roles = "user,admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Plot plot)
    {
        plot.Id = id;
        var ok = await _repo.UpdateAsync(plot);
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

