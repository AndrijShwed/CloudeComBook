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
    public async Task<List<Person>?> GetPeopleAsync() =>
        await _http.GetFromJsonAsync<List<Person>>("/api/people");
}
