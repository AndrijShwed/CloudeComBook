using ClaudeComBook.Shared.Models;
using ClaudeComBook.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClaudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentTemplatesController : ControllerBase
{
    private readonly IDocumentTemplateRepository _repo;

    public DocumentTemplatesController(IDocumentTemplateRepository repo)
    {
        _repo = repo;
    }

    [HttpPost("upload/{type}")]
    public async Task<IActionResult> Upload(string type, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Файл не вибрано.");

        if (!Path.GetExtension(file.FileName)
        .Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Дозволено завантажувати лише файли Word (*.docx).");
        }

        var templatesFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "DocTemplates");

        Directory.CreateDirectory(templatesFolder);

        string fileName = $"{type}.docx";
        string filePath = Path.Combine(templatesFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var template = await _repo.GetByTypeAsync(type);

        if (template == null)
        {
            await _repo.CreateAsync(new DocumentTemplate
            {
                Name = fileName,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            template.Name = fileName;
            template.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(template);
        }

        return Ok();
    }

    [HttpGet("download/{type}")]
    public async Task<IActionResult> Download(string type)
    {
        var template = await _repo.GetByTypeAsync(type);

        if (template == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(template.Name))
            return NotFound();

        string filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "DocTemplates",
            template.Name);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        byte[] bytes = await System.IO.File.ReadAllBytesAsync(filePath);

        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            template.Name);
    }

    [HttpGet("exists/{type}")]
    public async Task<IActionResult> Exists(string type)
    {
        var template = await _repo.GetByTypeAsync(type);

        if (template == null || string.IsNullOrWhiteSpace(template.Name))
            return Ok(false);

        string filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "DocTemplates",
            template.Name);

        return Ok(System.IO.File.Exists(filePath));
    }
}

