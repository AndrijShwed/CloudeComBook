using ClaudeComBook.Shared.DTOs;
using ClaudeComBook.Shared.DTOs.Auth;
using ClaudeComBook.Shared.Filters;
using ClaudeComBook.Shared.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;
using System.Net.Http.Headers;

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

    public async Task<UserProfileResponse?> GetProfileAsync(int userId)
    {
        var response = await _http.GetAsync($"api/auth/users/{userId}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserProfileResponse>();
    }

    public async Task<bool> UpdateProfileAsync(int userId, UpdateSettingsRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/auth/users/{userId}/settings", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<UserProfileResponse>> GetUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<UserProfileResponse>>("api/auth/users")
               ?? new List<UserProfileResponse>();
    }

    public async Task<bool> RegisterUserAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/auth/users/{id}");

        if (response.IsSuccessStatusCode)
            return (true, null);

        string? error = null;
        try
        {
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            body?.TryGetValue("message", out error);
        }
        catch { /* ігноруємо, якщо тіло не JSON */ }

        return (false, error);
    }

    public async Task<bool> ToggleUserActiveAsync(int id, bool isActive)
    {
        var response = await _http.PutAsJsonAsync($"api/auth/users/{id}/toggle", isActive);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<DocumentTemplateStatusDto>> GetTemplateStatusesAsync()
    {
        return await _http.GetFromJsonAsync<List<DocumentTemplateStatusDto>>("api/documenttemplates/status")
               ?? new List<DocumentTemplateStatusDto>();
    }

    public async Task<bool> UploadTemplateAsync(string type, IBrowserFile file)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024));
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType is { Length: > 0 } ? file.ContentType : "application/octet-stream");
        content.Add(streamContent, "file", file.Name);

        var response = await _http.PostAsync($"api/documenttemplates/upload/{Uri.EscapeDataString(type)}", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<(byte[] Bytes, string FileName)?> DownloadTemplateAsync(string type)
    {
        var response = await _http.GetAsync($"api/documenttemplates/download/{Uri.EscapeDataString(type)}");
        if (!response.IsSuccessStatusCode)
            return null;

        var bytes = await response.Content.ReadAsByteArrayAsync();
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                       ?? response.Content.Headers.ContentDisposition?.FileName
                       ?? $"{type}.docx";
        fileName = fileName.Trim('"');

        return (bytes, fileName);
    }

    public async Task<List<Street>> GetStreetsAsync()
    {
        return await _http.GetFromJsonAsync<List<Street>>("api/streets")
               ?? new List<Street>();
    }

    public async Task<bool> CreateVillageAsync(Village village)
    {
        var response = await _http.PostAsJsonAsync("api/villages", village);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateVillageAsync(Village village)
    {
        var response = await _http.PutAsJsonAsync($"api/villages/{village.Id}", village);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteVillageAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/villages/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateStreetAsync(Street street)
    {
        var response = await _http.PostAsJsonAsync("api/streets", street);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateStreetAsync(Street street)
    {
        var response = await _http.PutAsJsonAsync($"api/streets/{street.Id}", street);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteStreetAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/streets/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<VillageStreet>> GetVillageStreetsAsync()
    {
        return await _http.GetFromJsonAsync<List<VillageStreet>>("api/villagestreets")
               ?? new List<VillageStreet>();
    }
    public async Task<bool> CreateVillageStreetAsync(VillageStreet villageStreet)
    {
        var response = await _http.PostAsJsonAsync("api/villagestreets", villageStreet);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateVillageStreetAsync(VillageStreet villageStreet)
    {
        var response = await _http.PutAsJsonAsync($"api/villagestreets/{villageStreet.Id}", villageStreet);
        return response.IsSuccessStatusCode;
    }
    public async Task<Village?> CreateVillageAndReturnAsync(Village village)
    {
        var response = await _http.PostAsJsonAsync("api/villages", village);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<Village>();
    }

    public async Task<Street?> CreateStreetAndReturnAsync(Street street)
    {
        var response = await _http.PostAsJsonAsync("api/streets", street);
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<Street>();
    }

    public async Task<List<House>> GetHousesByVillageStreetIdAsync(int villageStreetId)
    {
        return await _http.GetFromJsonAsync<List<House>>($"api/houses/by-villagestreet/{villageStreetId}")
               ?? new List<House>();
    }

    public async Task<bool> CreateHouseAsync(House house)
    {
        var response = await _http.PostAsJsonAsync("api/houses", house);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateHouseAsync(House house)
    {
        var response = await _http.PutAsJsonAsync($"api/houses/{house.IdHouses}", house);
        return response.IsSuccessStatusCode;
    }

    public async Task<(bool Success, string? Error)> DeleteHouseAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/houses/{id}");

        if (response.IsSuccessStatusCode)
            return (true, null);

        string? error = null;
        try
        {
            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            body?.TryGetValue("message", out error);
        }
        catch { /* ігноруємо, якщо тіло не JSON */ }

        return (false, error);
    }
    public async Task<bool> CreatePersonAsync(Person person)
    {
        var response = await _http.PostAsJsonAsync("api/people", person);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdatePersonAsync(Person person)
    {
        var response = await _http.PutAsJsonAsync($"api/people/{person.PeopleId}", person);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePersonAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/people/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PersonExistsAsync(string lastName, string name, string? surname, DateTime? dateOfBirth)
    {
        var query = new List<string>
    {
        $"lastName={Uri.EscapeDataString(lastName)}",
        $"name={Uri.EscapeDataString(name)}"
    };

        if (!string.IsNullOrWhiteSpace(surname))
            query.Add($"surname={Uri.EscapeDataString(surname)}");

        if (dateOfBirth.HasValue)
            query.Add($"dateOfBirth={dateOfBirth.Value:yyyy-MM-dd}");

        var url = "api/people/exists?" + string.Join("&", query);

        return await _http.GetFromJsonAsync<bool>(url);
    }
}