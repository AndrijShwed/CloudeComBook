using ClaudeComBook.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClaudeComBook.Desktop.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "https://localhost:7079/api";

    public ApiService()
    {
        _http = new HttpClient();
        // Ігноруємо SSL помилки для локальної розробки
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        _http = new HttpClient(handler);
        _http.BaseAddress = new Uri(BaseUrl);
    }

    // Villages
    public async Task<List<Village>?> GetVillagesAsync() =>
        await _http.GetFromJsonAsync<List<Village>>("/api/villages");

    // Streets
    public async Task<List<Street>?> GetStreetsAsync() =>
        await _http.GetFromJsonAsync<List<Street>>("/api/streets");

    //Person
    public async Task<List<Person>?> GetPeopleAsync(
    string? lastName = null,
    string? name = null,
    string? surname = null,
    string? sex = null,
    string? status = null,
    string? registr = null,
    int? villageStreetId = null,
    string? houseNumb = null,
    int? ageFrom = null,
    int? ageTo = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrEmpty(lastName)) query.Add($"lastName={Uri.EscapeDataString(lastName)}");
        if (!string.IsNullOrEmpty(name)) query.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(surname)) query.Add($"surname={Uri.EscapeDataString(surname)}");
        if (!string.IsNullOrEmpty(sex)) query.Add($"sex={Uri.EscapeDataString(sex)}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrEmpty(registr)) query.Add($"registr={Uri.EscapeDataString(registr)}");
        if (villageStreetId.HasValue) query.Add($"villageStreetId={villageStreetId}");
        if (!string.IsNullOrEmpty(houseNumb)) query.Add($"houseNumb={Uri.EscapeDataString(houseNumb)}");
        if (ageFrom.HasValue) query.Add($"ageFrom={ageFrom}");
        if (ageTo.HasValue) query.Add($"ageTo={ageTo}");

        var url = "/api/people";
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return await _http.GetFromJsonAsync<List<Person>>(url);
    }

    // ApiService.cs
    public async Task<List<VillageStreet>?> GetVillageStreetsAsync() =>
        await _http.GetFromJsonAsync<List<VillageStreet>>("/api/villagestreets");
}
