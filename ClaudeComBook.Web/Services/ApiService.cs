using ClaudeComBook.Shared.Filters;
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

    public async Task<List<Person>> GetPeopleAsync(PersonFilter? filter = null)
    {
        filter ??= new PersonFilter();

        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.LastName))
            query.Add($"lastName={Uri.EscapeDataString(filter.LastName)}");

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query.Add($"name={Uri.EscapeDataString(filter.Name)}");

        if (!string.IsNullOrWhiteSpace(filter.Surname))
            query.Add($"surname={Uri.EscapeDataString(filter.Surname)}");

        if (!string.IsNullOrWhiteSpace(filter.Sex))
            query.Add($"sex={Uri.EscapeDataString(filter.Sex)}");

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query.Add($"status={Uri.EscapeDataString(filter.Status)}");

        if (!string.IsNullOrWhiteSpace(filter.Registr))
            query.Add($"registr={Uri.EscapeDataString(filter.Registr)}");

        if (filter.VillageId.HasValue)
            query.Add($"villageId={filter.VillageId}");

        if (filter.StreetId.HasValue)
            query.Add($"streetId={filter.StreetId}");

        if (!string.IsNullOrWhiteSpace(filter.HouseNumb))
            query.Add($"houseNumb={Uri.EscapeDataString(filter.HouseNumb)}");

        if (filter.AgeFrom.HasValue)
            query.Add($"ageFrom={filter.AgeFrom}");

        if (filter.AgeTo.HasValue)
            query.Add($"ageTo={filter.AgeTo}");

        if (filter.StatusYear.HasValue)
            query.Add($"statusYear={filter.StatusYear}");

        var url = "api/people";

        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        return await _http.GetFromJsonAsync<List<Person>>(url)
               ?? new List<Person>();
    }

    public async Task<List<Village>> GetVillagesAsync()
    {
        return await _http.GetFromJsonAsync<List<Village>>("api/villages")
               ?? new List<Village>();
    }
}