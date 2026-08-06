using ClaudeComBook.Shared.Filters;
using ClaudeComBook.Shared.Models;
using ClaudeComBook.Web.Services;

namespace ClaudeComBook.Web.ViewModels;

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

    private bool _loading;

    public bool Loading
    {
        get => _loading;
        set => SetProperty(ref _loading, value);
    }

    public async Task InitializeAsync()
    {
        Villages = await _api.GetVillagesAsync();

        await LoadPeopleAsync();
    }

    public async Task LoadPeopleAsync()
    {
        Loading = true;

        People = await _api.GetPeopleAsync(Filter);

        Loading = false;
    }
    public void ClearFilter()
    {
        Filter.LastName = string.Empty;
        Filter.Name = string.Empty;
        Filter.Surname = string.Empty;
        Filter.Sex = null;
        Filter.Status = null;
        Filter.Registr = null;
        Filter.VillageId = null;
        Filter.StreetId = null;
        Filter.HouseNumb = string.Empty;
        Filter.AgeFrom = null;
        Filter.AgeTo = null;
        Filter.StatusYear = null;
    }
}
