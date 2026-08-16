using CloudeComBook.API.Helpers;
using CloudeComBook.API.Repositories.Interfaces;
using CloudeComBook.API.Services;
using CloudeComBook.Shared.Constants;
using CloudeComBook.Shared.DTOs;
using CloudeComBook.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CloudeComBook.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Authorize(Roles = "user,admin")]
public class DocumentsController : ControllerBase
{
    private readonly IPersonRepository _personRepo;
    private readonly IHouseRepository _houseRepo;
    private readonly IDocumentTemplateRepository _templateRepo;
    private readonly IUserRepository _userRepo; // припущення: репозиторій користувачів з профілем
    private readonly OpenXmlDocumentService _docService = new();

    public DocumentsController(
        IPersonRepository personRepo,
        IHouseRepository houseRepo,
        IDocumentTemplateRepository templateRepo,
        IUserRepository userRepo)
    {
        _personRepo = personRepo;
        _houseRepo = houseRepo;
        _templateRepo = templateRepo;
        _userRepo = userRepo;
    }


    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateDocumentRequest request)
    {
        var person = await _personRepo.GetByIdAsync(request.PersonId);
        if (person == null) return NotFound("Особу не знайдено.");

        var template = await _templateRepo.GetByTypeAsync(request.TemplateType);
        if (template == null || string.IsNullOrWhiteSpace(template.Name))
            return BadRequest("Шаблон документу не знайдено. Завантажте шаблон через адмін панель.");

        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "DocTemplates", template.Name);
        if (!System.IO.File.Exists(templatePath))
            return BadRequest("Файл шаблону відсутній на сервері.");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value;
        var currentUser = int.TryParse(userIdClaim, out var userId)
            ? await _userRepo.GetByIdAsync(userId)
            : null;

        var fields = new Dictionary<string, string>();

        if (person.Sex != "чол")
        {
            fields["his"] = "її";
            fields["зареєстрований"] = "зареєстрована";
            fields["який народився"] = "яка народилася";
            fields["him"] = "нею";
            fields["жителю"] = "жительці";
            fields["його"] = "її";
            fields["жителя"] = "жительку";
            fields["which born"] = "яка народилась";
            fields["registr"] = "зареєстрована";
        }
        else
        {
            fields["his"] = "його";
            fields["him"] = "ним";
            fields["which born"] = "який народився";
            fields["registr"] = "зареєстрований";
            fields["жителю"] = "жителю";
        }

        var position = currentUser?.Position ?? "";
        fields["Посада_1"] = position;
        fields["Посада"] = position.Length >= 8 ? position.Substring(0, 8) : position;
        fields["Name_1 SURNAME_1"] = currentUser?.FullName ?? "";
        fields["ShortName"] = currentUser?.ShortName ?? "";
        fields["ПоточнаДата"] = DateTime.Now.ToString("dd.MM.yyyy");
        fields["village"] = person.VillageName ?? "";
        fields["street"] = person.StreetName ?? "";
        fields["house"] = person.NumbOfHouse ?? "";
        fields["full_name"] = $"{person.LastName} {person.Name} {person.Surname}";
        fields["birth_date"] = person.DateOfBirth?.ToString("dd.MM.yyyy") ?? "";

        foreach (var f in request.ExtraFields)
            fields[f.Key] = f.Value;

        if (fields.TryGetValue("ПоштовийКодЗаповідача", out var postalCode))
        {
            postalCode = (postalCode ?? "").Trim();
            for (int i = 0; i < 5; i++)
                fields[$"p-{i + 1}"] = postalCode.Length > i ? postalCode[i].ToString() : "";
        }

        if (fields.TryGetValue("ДатаРеєстраціїЗаповіту", out var registrationDate))
        {
            if (DateTime.TryParse(registrationDate, out var date))
            {
                fields["dR"] = date.ToString("dd");
                fields["mR"] = date.ToString("MM");
                fields["yR"] = date.ToString("yyyy");
            }
            else
            {
                fields["dR"] = fields["mR"] = fields["yR"] = "";
            }
        }

        List<Shared.Models.Person>? familyMembers = null;
        if (person.VillageStreetId.HasValue)
        {
            familyMembers = (await _personRepo.GetByAddressAsync(
                person.VillageStreetId.Value, person.NumbOfHouse ?? "", person.PeopleId)).ToList();
        }

        if (request.TemplateType == DocumentTemplateTypes.FamilyComposition && person.VillageStreetId.HasValue)
        {
            var house = (await _houseRepo.GetByVillageStreetIdAsync(person.VillageStreetId.Value))
                .FirstOrDefault(h => h.NumbOfHouse == person.NumbOfHouse);

            fields["ЗагальнаПлоща"] = house?.TotalArea?.ToString("F1") ?? "0";

            fields["що його сім'я складається з осіб:"] = familyMembers != null && familyMembers.Count > 0
                ? (person.Sex != "чол" ? "що її сім'я складається з осіб:" : "що його сім'я складається з осіб:")
                : "за даною адресою особа проживає одна";
        }

        if (request.TemplateType == DocumentTemplateTypes.Testament)
        {
            fields["ДатаТекст"] = UkrainianDateFormatter.GetDateInWords(DateTime.Now);
        }

        if (request.TemplateType is DocumentTemplateTypes.Subsidy or DocumentTemplateTypes.Benefits
            && person.VillageStreetId.HasValue)
        {
            var house = (await _houseRepo.GetByVillageStreetIdAsync(person.VillageStreetId.Value))
                .FirstOrDefault(h => h.NumbOfHouse == person.NumbOfHouse);

            fields["ЗагальнаПлоща"] = house?.TotalArea?.ToString("F1") ?? "0";
            fields["ЖитловаПлоща"] = house?.LivingArea?.ToString("F1") ?? "0";

            int countReal = (familyMembers?.Count ?? 0) + 1;
            fields["curentMonth"] = DateTime.Now.Month.ToString("00");
            fields["curentYear"] = DateTime.Now.Year.ToString();
            fields["Кількість"] = countReal.ToString();
            fields["Кільк.Прописом"] = CountToWords(countReal);
            fields["Документ"] = OpenXmlDocumentService.FormatDocument(person.Passport);
        }

        if (request.TemplateType == DocumentTemplateTypes.TestamentRegistration)
        {
            var idKod = person.IdKod?.Trim() ?? "";
            if (idKod.Length != 10 && idKod.Length > 0)
                return BadRequest("Неправильно введений ідентифікаційний код.");

            if (idKod.Length == 10)
            {
                for (int i = 0; i < 9; i++)
                    fields[$"i-{i + 1}"] = idKod.Substring(i, 1);
                fields["i-0"] = idKod.Substring(9, 1);
            }

            fields["dB"] = person.DateOfBirth?.ToString("dd") ?? "";
            fields["mB"] = person.DateOfBirth?.ToString("MM") ?? "";
            fields["yB"] = person.DateOfBirth?.ToString("yyyy") ?? "";

            var postIndex = currentUser?.PostIndex ?? "";
            for (int i = 0; i < 5; i++)
                fields[$"z-{i + 1}"] = postIndex.Length > i ? postIndex[i].ToString() : "";

            fields["ruegion"] = currentUser?.Region ?? "";
            fields["duistrict"] = currentUser?.District ?? "";
            fields["fuull_name"] = currentUser?.FullName ?? "";
            fields["puosition"] = currentUser?.Position ?? "";
            fields["ourganization"] = currentUser?.Organization ?? "";
            fields["vuillage"] = currentUser?.Village ?? "";
            fields["sutreet"] = currentUser?.Street ?? "";
            fields["uah"] = currentUser?.House ?? "";
            fields["puhone"] = currentUser?.Phone ?? "";
        }

        var templateBytes = await System.IO.File.ReadAllBytesAsync(templatePath);
        var resultBytes = _docService.GenerateDocument(templateBytes, fields, familyMembers);

        var fileName = $"{person.LastName}_{person.Name}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";

        return File(resultBytes,
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            fileName);
    }

    private static string CountToWords(int count) => count switch
    {
        1 => "(одна) особа",
        2 => "(дві) особи",
        3 => "(три) особи",
        4 => "(чотири) особи",
        5 => "(п'ять) осіб",
        6 => "(шість) осіб",
        7 => "(сім) осіб",
        8 => "(вісім) осіб",
        9 => "(дев'ять) осіб",
        10 => "(десять) осіб",
        _ => $"({count}) осіб"
    };
}
