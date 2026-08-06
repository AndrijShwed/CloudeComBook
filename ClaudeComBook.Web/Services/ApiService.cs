using ClaudeComBook.Shared.Models;
using System.Net.Http.Json;

namespace ClaudeComBook.Web.Services;

public class ApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Person>> GetPeopleAsync()
    {
        return await _http.GetFromJsonAsync<List<Person>>("api/people")
               ?? new List<Person>();
    }
}