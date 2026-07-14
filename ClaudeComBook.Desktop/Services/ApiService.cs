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
    int? villageId = null,
    int? streetId = null,
    string? houseNumb = null,
    int? ageFrom = null,
    int? ageTo = null,
    int? statusYear = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrEmpty(lastName)) query.Add($"lastName={Uri.EscapeDataString(lastName)}");
        if (!string.IsNullOrEmpty(name)) query.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(surname)) query.Add($"surname={Uri.EscapeDataString(surname)}");
        if (!string.IsNullOrEmpty(sex)) query.Add($"sex={Uri.EscapeDataString(sex)}");
        if (!string.IsNullOrEmpty(status)) query.Add($"status={Uri.EscapeDataString(status)}");
        if (!string.IsNullOrEmpty(registr)) query.Add($"registr={Uri.EscapeDataString(registr)}");
        if (villageId.HasValue) query.Add($"villageId={villageId}");
        if (streetId.HasValue) query.Add($"streetId={streetId}");
        if (!string.IsNullOrEmpty(houseNumb)) query.Add($"houseNumb={Uri.EscapeDataString(houseNumb)}");
        if (ageFrom.HasValue) query.Add($"ageFrom={ageFrom}");
        if (ageTo.HasValue) query.Add($"ageTo={ageTo}");
        if (statusYear.HasValue) query.Add($"statusYear={statusYear}");

        var url = "/api/people";
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return await _http.GetFromJsonAsync<List<Person>>(url);
    }

    // ApiService.cs
    public async Task<List<VillageStreet>?> GetVillageStreetsAsync() =>
        await _http.GetFromJsonAsync<List<VillageStreet>>("/api/villagestreets");

    public async Task<int> CreateVillageAsync(string name)
    {
        var response = await _http.PostAsJsonAsync("/api/villages", new { name });
        var village = await response.Content.ReadFromJsonAsync<Village>();
        return village?.Id ?? 0;
    }

    public async Task<int> CreateStreetAsync(string name)
    {
        var response = await _http.PostAsJsonAsync("/api/streets", new { name });
        var street = await response.Content.ReadFromJsonAsync<Street>();
        return street?.Id ?? 0;
    }

    public async Task CreateVillageStreetAsync(int villageId, int streetId)
    {
        await _http.PostAsJsonAsync("/api/villagestreets", new { villageId, streetId, isActive = true });
    }

    public async Task DeleteVillageStreetAsync(int id)
    {
        await _http.DeleteAsync($"/api/villagestreets/{id}");
    }

    public async Task UpdateVillageStreetFileAsync(int id, byte[] fileData)
    {
        await _http.PutAsJsonAsync($"/api/villagestreets/{id}/file",
            new { fileData = Convert.ToBase64String(fileData) });
    }
    public async Task UpdatePersonAsync(Person person)
    {
        await _http.PutAsJsonAsync($"/api/people/{person.PeopleId}", person);
    }
    public async Task CreatePersonAsync(Person person)
    {
        await _http.PostAsJsonAsync("/api/people", person);
    }

    public async Task DeletePersonAsync(int id)
    {
        await _http.DeleteAsync($"/api/people/{id}");
    }

    public async Task RenameStreetAsync(int villageId, int oldStreetId,
    int newStreetId, DateTime? renameDate, byte[]? fileData)
    {
        var request = new
        {
            villageId,
            oldStreetId,
            newStreetId,
            renameDate,
            fileData = fileData != null ? Convert.ToBase64String(fileData) : null
        };
        await _http.PostAsJsonAsync("/api/villagestreets/rename", request);
    }

    public async Task<List<PopulationSnapshot>?> GetPopulationSnapshotsAsync() =>
    await _http.GetFromJsonAsync<List<PopulationSnapshot>>("/api/populationsnapshots");
    public async Task<Dictionary<string, int>?> GetCurrentPopulationAsync() =>
    await _http.GetFromJsonAsync<Dictionary<string, int>>("/api/people/population-by-village");

    public async Task SavePopulationSnapshotAsync(PopulationRow row, List<string> villages)
    {
        var currentYear = System.DateTime.Now.Year;
        foreach (var village in villages)
        {
            var population = row.VillagePopulations.GetValueOrDefault(village, 0);
            await _http.PostAsJsonAsync("/api/populationsnapshots/upsert", new
            {
                settlementName = village,
                year = currentYear,
                population,
                createdAt = System.DateTime.Now
            });
        }
    }
    public async Task CreateHouseAsync(House house)
    {
        await _http.PostAsJsonAsync("/api/houses", house);
    }
    public async Task<bool> HouseExistsAsync(int villageStreetId, string numbOfHouse)
    {
        var result = await _http.GetFromJsonAsync<bool>(
            $"/api/houses/exists?villageStreetId={villageStreetId}&numbOfHouse={Uri.EscapeDataString(numbOfHouse)}");
        return result;
    }
    public async Task<List<House>?> GetHousesByVillageStreetAsync(int villageStreetId) =>
    await _http.GetFromJsonAsync<List<House>>($"/api/houses/by-villagestreet/{villageStreetId}");

    public async Task<List<House>?> SearchHousesAsync(
        int? villageId = null,
        int? streetId = null,
        string? houseNumber = null,
        string? lastName = null,
        string? name = null,
        string? surname = null)
    {
        var query = new System.Collections.Generic.List<string>();
        if (villageId.HasValue) query.Add($"villageId={villageId}");
        if (streetId.HasValue) query.Add($"streetId={streetId}");
        if (!string.IsNullOrEmpty(houseNumber)) query.Add($"houseNumber={Uri.EscapeDataString(houseNumber)}");
        if (!string.IsNullOrEmpty(lastName)) query.Add($"lastName={Uri.EscapeDataString(lastName)}");
        if (!string.IsNullOrEmpty(name)) query.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(surname)) query.Add($"surname={Uri.EscapeDataString(surname)}");

        var url = "/api/houses/search";
        if (query.Count > 0) url += "?" + string.Join("&", query);
        return await _http.GetFromJsonAsync<List<House>>(url);
    }

    public async Task DeleteHouseAsync(int id)
    {
        await _http.DeleteAsync($"/api/houses/{id}");
    }
    public async Task UpdateHouseAsync(House house)
    {
        await _http.PutAsJsonAsync($"/api/houses/{house.IdHouses}", house);
    }
    public class VillageAreaData
    {
        public string Village { get; set; } = "";
        public decimal TotalArea { get; set; }
        public decimal LivingArea { get; set; }
    }

    public async Task<List<VillageAreaData>?> GetAreaByVillageAsync() =>
        await _http.GetFromJsonAsync<List<VillageAreaData>>("/api/houses/area-by-village");
    public class VillageRoomsData
    {
        public string Village { get; set; } = "";
        public int OneRoom { get; set; }
        public int TwoRooms { get; set; }
        public int ThreeRooms { get; set; }
        public int FourRooms { get; set; }
        public int FiveRooms { get; set; }
        public int SixRooms { get; set; }
        public int MoreThanSix { get; set; }
        public int Total { get; set; }
    }

    public async Task<List<VillageRoomsData>?> GetRoomsByVillageAsync() =>
        await _http.GetFromJsonAsync<List<VillageRoomsData>>("/api/houses/rooms-by-village");

    public async Task CreateEnterpriseAsync(Enterprise enterprise)
    {
        await _http.PostAsJsonAsync("/api/enterprises", enterprise);
    }
    public async Task<List<Enterprise>?> SearchEnterprisesAsync(
    string? name = null,
    string? owner = null,
    int? villageId = null,
    int? streetId = null,
    string? houseNumber = null)
    {
        var query = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrEmpty(name)) query.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(owner)) query.Add($"owner={Uri.EscapeDataString(owner)}");
        if (villageId.HasValue) query.Add($"villageId={villageId}");
        if (streetId.HasValue) query.Add($"streetId={streetId}");
        if (!string.IsNullOrEmpty(houseNumber)) query.Add($"houseNumber={Uri.EscapeDataString(houseNumber)}");

        var url = "/api/enterprises/search";
        if (query.Count > 0) url += "?" + string.Join("&", query);
        return await _http.GetFromJsonAsync<List<Enterprise>>(url);
    }

    public async Task DeleteEnterpriseAsync(int id)
    {
        await _http.DeleteAsync($"/api/enterprises/{id}");
    }
    public async Task UpdateEnterpriseAsync(Enterprise enterprise)
    {
        await _http.PutAsJsonAsync($"/api/enterprises/{enterprise.Id}", enterprise);
    }
    public async Task<bool> EnterpriseExistsByNameAsync(string name)
    {
        return await _http.GetFromJsonAsync<bool>(
            $"/api/enterprises/exists?name={Uri.EscapeDataString(name)}");
    }
    public async Task<bool> PersonExistsAsync(string lastName, string name, string? surname, DateTime? dateOfBirth)
    {
        var query = $"lastName={Uri.EscapeDataString(lastName)}&name={Uri.EscapeDataString(name)}";
        if (!string.IsNullOrEmpty(surname)) query += $"&surname={Uri.EscapeDataString(surname)}";
        if (dateOfBirth.HasValue) query += $"&dateOfBirth={dateOfBirth.Value:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<bool>($"/api/people/exists?{query}");
    }
    public async Task<bool> AnymalExistsAsync(string lastName, string name, string? surname, string village)
    {
        var query = $"lastName={Uri.EscapeDataString(lastName)}&name={Uri.EscapeDataString(name)}&village={Uri.EscapeDataString(village)}";
        if (!string.IsNullOrEmpty(surname)) query += $"&surname={Uri.EscapeDataString(surname)}";
        return await _http.GetFromJsonAsync<bool>($"/api/anymals/exists?{query}");
    }

    public async Task CreateAnymalAsync(Anymal anymal)
    {
        await _http.PostAsJsonAsync("/api/anymals", anymal);
    }
    public async Task<List<Anymal>?> SearchAnymalsAsync(
    string? lastName = null,
    string? name = null,
    string? surname = null,
    string? village = null)
    {
        var query = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrEmpty(lastName)) query.Add($"lastName={Uri.EscapeDataString(lastName)}");
        if (!string.IsNullOrEmpty(name)) query.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(surname)) query.Add($"surname={Uri.EscapeDataString(surname)}");
        if (!string.IsNullOrEmpty(village)) query.Add($"village={Uri.EscapeDataString(village)}");

        var url = "/api/anymals/search";
        if (query.Count > 0) url += "?" + string.Join("&", query);
        return await _http.GetFromJsonAsync<List<Anymal>>(url);
    }

    public async Task DeleteAnymalAsync(int id)
    {
        await _http.DeleteAsync($"/api/anymals/{id}");
    }
    public async Task UpdateAnymalAsync(Anymal anymal)
    {
        await _http.PutAsJsonAsync($"/api/anymals/{anymal.AnymalsId}", anymal);
    }
    public class AnymalStatData
    {
        public string Village { get; set; } = "";
        public int Covs { get; set; }
        public int Pigs { get; set; }
        public int Sheeps { get; set; }
        public int Goats { get; set; }
        public int Horses { get; set; }
        public int Birds { get; set; }
        public int Rabbits { get; set; }
        public int Beeses { get; set; }
        public int Anymals { get; set; }
    }

    public async Task<List<AnymalStatData>?> GetAnymalStatisticsAsync() =>
        await _http.GetFromJsonAsync<List<AnymalStatData>>("/api/anymals/statistics");
    public async Task CreatePlotAsync(Plot plot)
    {
       var response =  await _http.PostAsJsonAsync("/api/plot", plot);

        var text = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}\n\n{text}");
        }
    }
}
