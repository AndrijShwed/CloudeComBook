using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using ClaudeComBook.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentTemplatesController : ControllerBase
{
    private readonly IDocumentTemplateRepository _repo;
    public DocumentTemplatesController(IDocumentTemplateRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _repo.GetAllAsync());

    [HttpGet("by-type/{type}")]
    public async Task<IActionResult> GetByType(string type)
    {
        var template = await _repo.GetByTypeAsync(type);
        return template == null ? NotFound() : Ok(template);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var template = await _repo.GetByIdAsync(id);
        return template == null ? NotFound() : Ok(template);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DocumentTemplate template)
    {
        template.Id = await _repo.CreateAsync(template);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DocumentTemplate template)
    {
        template.Id = id;
        var ok = await _repo.UpdateAsync(template);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] DocumentTemplateUploadDto dto)
    {
        var templateBytes = Convert.FromBase64String(dto.Template);
        var existing = await _repo.GetByTypeAsync(dto.Type);

        if (existing != null)
        {
            existing.Name = dto.Name;
            existing.Template = templateBytes;
            var ok = await _repo.UpdateAsync(existing);
            return Ok(new { updated = true, id = existing.Id, success = ok });
        }
        else
        {
            var id = await _repo.CreateAsync(new DocumentTemplate
            {
                Name = dto.Name,
                Type = dto.Type,
                Template = templateBytes
            });
            return Ok(new { updated = false, id });
        }
    }
}
