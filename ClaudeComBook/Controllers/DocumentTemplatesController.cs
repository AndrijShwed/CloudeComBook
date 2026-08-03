//using ClaudeComBook.API.Models;
//using ClaudeComBook.API.Repositories.Interfaces;
//using ClaudeComBook.DTOs;
//using Microsoft.AspNetCore.Mvc;

//namespace ClaudeComBook.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class DocumentTemplatesController : ControllerBase
//{
//    private readonly IDocumentTemplateRepository _repo;
//    public DocumentTemplatesController(IDocumentTemplateRepository repo) => _repo = repo;

//    //[HttpGet]
//    //public async Task<IActionResult> GetAll() =>
//    //    Ok(await _repo.GetAllAsync());

//    //[HttpGet("{id}")]
//    //public async Task<IActionResult> GetById(int id)
//    //{
//    //    var template = await _repo.GetByIdAsync(id);
//    //    return template == null ? NotFound() : Ok(template);
//    //}

//    //[HttpPost]
//    //public async Task<IActionResult> Create([FromForm] CreateDocumentTemplateRequest request)
//    //{
//    //    if (request.File == null || request.File.Length == 0)
//    //        return BadRequest("Файл не вибрано.");

//    //    // Перевірка розширення
//    //    if (Path.GetExtension(request.File.FileName).ToLowerInvariant() != ".docx")
//    //        return BadRequest("Дозволено завантажувати лише файли .docx.");


//    //    string templatesFolder = Path.Combine(
//    //        Directory.GetCurrentDirectory(),
//    //        "DocTemplates");

//    //    Directory.CreateDirectory(templatesFolder);

//    //    string fileName = $"{request.Name}.docx";

//    //    string destination = Path.Combine(templatesFolder, fileName);

//    //    await using (var stream = new FileStream(destination, FileMode.Create))
//    //    {
//    //        await request.File.CopyToAsync(stream);
//    //    }

//    //    var template = new DocumentTemplate
//    //    {
//    //        Name = request.Name,
//    //        Type = request.Type,
//    //        CreatedAt = DateTime.Now,
//    //        UpdatedAt = DateTime.Now
//    //    };

//    //    template.Id = await _repo.CreateAsync(template);

//    //    return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
//    //}

//    //[HttpPut("{id}")]
//    //public async Task<IActionResult> Update(int id, [FromBody] DocumentTemplate template)
//    //{
//    //    template.Id = id;
//    //    var ok = await _repo.UpdateAsync(template);
//    //    return ok ? NoContent() : NotFound();
//    //}

//    //[HttpDelete("{id}")]
//    //public async Task<IActionResult> Delete(int id)
//    //{
//    //    var ok = await _repo.DeleteAsync(id);
//    //    return ok ? NoContent() : NotFound();
//    //}

//    //[HttpPost("upsert")]
//    //public async Task<IActionResult> Upsert([FromBody] DocumentTemplatePathDto dto)
//    //{
//    //    var existing = await _repo.GetByTypeAsync(dto.Type);
//    //    if (existing != null)
//    //    {
//    //        existing.Name = dto.Name;
//    //        await _repo.UpdateAsync(existing);
//    //    }
//    //    else
//    //    {
//    //        await _repo.CreateAsync(new DocumentTemplate
//    //        {
//    //            Name = dto.Name,
//    //            Type = dto.Type,
//    //        });
//    //    }
//    //    return Ok();
//    //}

//    //[HttpGet("by-type/{type}")]
//    //public async Task<IActionResult> GetByType(string type)
//    //{
//    //    var template = await _repo.GetByTypeAsync(type);
//    //    return template == null ? NotFound() : Ok(template);
//    //}

//    [HttpPost("upload/{type}")]
//    public async Task<IActionResult> Upload(string type, IFormFile file)
//    {
//        if (file == null || file.Length == 0)
//            return BadRequest("Файл не завантажено");

//        var templatesFolder = Path.Combine(Directory.GetCurrentDirectory(), "DocTemplates");
//        Directory.CreateDirectory(templatesFolder);

//        var fileName = $"{type}.docx";
//        var filePath = Path.Combine(templatesFolder, fileName);

//        using (var stream = new FileStream(filePath, FileMode.Create))
//            await file.CopyToAsync(stream);

//        var existing = await _repo.GetByTypeAsync(type);
//        if (existing != null)
//        {
//            existing.Name = fileName;
//            await _repo.UpdateAsync(existing);
//        }
//        else
//        {
//            await _repo.CreateAsync(new DocumentTemplate
//            {
//                Name = fileName,
//                Type = type
//            });
//        }

//        return Ok();
//    }

//    [HttpGet("download/{type}")]
//    public async Task<IActionResult> Download(string type)
//    {
//        var template = await _repo.GetByTypeAsync(type);
//        if (template?.Name == null) return NotFound();

//        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "DocTemplates", template.Name);
//        if (!System.IO.File.Exists(filePath)) return NotFound();

//        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
//        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
//    }
//}

using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories;
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

