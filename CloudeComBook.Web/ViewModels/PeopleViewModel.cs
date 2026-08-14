using CloudeComBook.Shared.Filters;
using CloudeComBook.Shared.Models;
using CloudeComBook.Web.Services;

namespace CloudeComBook.Web.ViewModels;

public class PeopleViewModel : BaseViewModel
{
    private readonly ApiService _api;

    public PeopleViewModel(ApiService api)
    {
        _api = api;
    }

    public PersonFilter Filter { get; } = new();

    public List<Person> People { get; private set; } = new();

    public List<Village> Villages { get; private set; } = new();

    public List<VillageStreet> VillageStreets { get; private set; } = new();

    public List<House> Houses { get; private set; } = new();

    public bool RegisteredYes { get; set; } = true;
    public bool RegisteredNo { get; set; }

    private bool _loading;

    public bool Loading
    {
        get => _loading;
        set => SetProperty(ref _loading, value);
    }

    public async Task InitializeAsync()
    {
        Villages = await _api.GetVillagesAsync();
        VillageStreets = await _api.GetVillageStreetsAsync();
        UpdateRegistrFilter();
        //await LoadPeopleAsync();
    }

    public List<VillageStreet> GetStreetsForVillage(int? villageId) =>
        villageId == null
            ? new List<VillageStreet>()
            : VillageStreets.Where(vs => vs.VillageId == villageId && vs.IsActive).ToList();

    public async Task LoadPeopleAsync()
    {
        Loading = true;

        People = await _api.GetPeopleAsync(Filter);

        Loading = false;
    }

    public void OnVillageChanged(int? villageId)
    {
        Filter.VillageId = villageId;
        Filter.StreetId = null;
        Filter.HouseNumb = null;
        Houses = new List<House>();
    }

    public async Task OnStreetChangedAsync(int? streetId)
    {
        Filter.StreetId = streetId;
        Filter.HouseNumb = null;

        var villageStreet = VillageStreets.FirstOrDefault(
            vs => vs.VillageId == Filter.VillageId && vs.StreetId == streetId && vs.IsActive);

        Houses = villageStreet == null
            ? new List<House>()
            : (await _api.GetHousesByVillageStreetIdAsync(villageStreet.Id))
                .OrderBy(h => GetHouseNumberSortKey(h.NumbOfHouse))
                .ThenBy(h => h.NumbOfHouse, StringComparer.OrdinalIgnoreCase)
                .ToList();
    }

    private static int GetHouseNumberSortKey(string? numb)
    {
        if (string.IsNullOrWhiteSpace(numb))
            return int.MaxValue;

        var digits = new string(numb.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var n) ? n : int.MaxValue;
    }

    public void SetRegisteredYes(bool value)
    {
        RegisteredYes = value;
        UpdateRegistrFilter();
    }

    public void SetRegisteredNo(bool value)
    {
        RegisteredNo = value;
        UpdateRegistrFilter();
    }

    private void UpdateRegistrFilter()
    {
        // Обидва позначені (або жоден) -> без фільтра, показуємо всіх
        // Позначено лише один -> фільтруємо за конкретним значенням
        Filter.Registr = (RegisteredYes, RegisteredNo) switch
        {
            (true, false) => "так",
            (false, true) => "ні",
            _ => null
        };
    }

    public void ClearFilter()
    {
        Filter.LastName = string.Empty;
        Filter.Name = string.Empty;
        Filter.Surname = string.Empty;
        Filter.Sex = null;
        Filter.Status = null;
        Filter.Registr = RegisteredYes ? "так" : null;
        Filter.VillageId = null;
        Filter.StreetId = null;
        Filter.HouseNumb = string.Empty;
        Filter.AgeFrom = null;
        Filter.AgeTo = null;
        Filter.StatusYear = null;

        Houses = new List<House>();
        RegisteredYes = true;
        RegisteredNo = false;
    }
}