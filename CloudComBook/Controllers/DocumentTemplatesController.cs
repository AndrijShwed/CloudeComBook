using CloudComBook.API.Repositories.Interfaces;
using CloudComBook.API.Services;
using CloudComBook.Shared.Constants;
using CloudComBook.Shared.DTOs;
using CloudComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloudComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "admin")]          // перегляд статусу й завантаження готового шаблону
public class DocumentTemplatesController : ControllerBase
{
    private const long MaxTemplateSize = 20 * 1024 * 1024;

    private readonly IDocumentTemplateRepository _repo;
    private readonly IAuditService _audit;

    public DocumentTemplatesController(IDocumentTemplateRepository repo, IAuditService audit)
    {
        _repo = repo;
        _audit = audit;
    }

    private static string TemplatesFolder =>
        Path.Combine(Directory.GetCurrentDirectory(), "DocTemplates");

    // Шаблони спільні для всіх громад, тому змінювати їх може лише супер-адмін
    [Authorize(Roles = "superadmin")]
    [RequestSizeLimit(MaxTemplateSize)]
    [HttpPost("upload/{type}")]
    public async Task<IActionResult> Upload(string type, IFormFile file)
    {
        // тип береться лише зі списку відомих, у ім'я файлу довільний рядок не потрапляє
        if (!DocumentTemplateTypes.All.Contains(type))
            return BadRequest("Невідомий тип шаблону.");

        if (file == null || file.Length == 0)
            return BadRequest("Файл не вибрано.");

        if (file.Length > MaxTemplateSize)
            return BadRequest("Файл завеликий (максимум 20 МБ).");

        if (!Path.GetExtension(file.FileName).Equals(".docx", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Дозволено завантажувати лише файли Word (*.docx).");

        // .docx це zip-архів, тож має починатися з 'PK'
        using (var peek = file.OpenReadStream())
        {
            var header = new byte[2];
            var read = await peek.ReadAsync(header.AsMemory(0, 2));
            if (read < 2 || header[0] != 0x50 || header[1] != 0x4B)
                return BadRequest("Файл не є коректним документом Word.");
        }

        Directory.CreateDirectory(TemplatesFolder);

        string fileName = $"{type}.docx";
        string filePath = Path.Combine(TemplatesFolder, fileName);
        string tempPath = filePath + ".tmp";

        // спочатку у тимчасовий файл, щоб обірване завантаження не зіпсувало чинний шаблон
        await using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        System.IO.File.Move(tempPath, filePath, overwrite: true);

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
            await _audit.LogAsync("document_templates", null, "CREATE", null,
                new { type, fileName, size = file.Length });
        }
        else
        {
            var old = new { type, template.Name, template.UpdatedAt };

            template.Name = fileName;
            template.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(template);

            await _audit.LogAsync("document_templates", template.Id, "UPDATE", old,
                new { type, fileName, size = file.Length });
        }

        return Ok();
    }

    [HttpGet("download/{type}")]
    public async Task<IActionResult> Download(string type)
    {
        if (!DocumentTemplateTypes.All.Contains(type))
            return NotFound();

        var template = await _repo.GetByTypeAsync(type);

        if (template == null || string.IsNullOrWhiteSpace(template.Name))
            return NotFound();

        string filePath = Path.Combine(TemplatesFolder, template.Name);

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
        if (!DocumentTemplateTypes.All.Contains(type))
            return Ok(false);

        var template = await _repo.GetByTypeAsync(type);

        if (template == null || string.IsNullOrWhiteSpace(template.Name))
            return Ok(false);

        return Ok(System.IO.File.Exists(Path.Combine(TemplatesFolder, template.Name)));
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var result = new List<DocumentTemplateStatusDto>();

        foreach (var type in DocumentTemplateTypes.All)
        {
            var template = await _repo.GetByTypeAsync(type);
            bool exists = template != null && !string.IsNullOrWhiteSpace(template.Name);

            result.Add(new DocumentTemplateStatusDto
            {
                Type = type,
                Exists = exists,
                UpdatedAt = exists ? template!.UpdatedAt : null
            });
        }

        return Ok(result);
    }
}