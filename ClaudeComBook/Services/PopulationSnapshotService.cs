using ClaudeComBook.API.Models;
using ClaudeComBook.API.Repositories.Interfaces;

namespace ClaudeComBook.API.Services;

public class PopulationSnapshotService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PopulationSnapshotService> _logger;

    public PopulationSnapshotService(
        IServiceProvider serviceProvider,
        ILogger<PopulationSnapshotService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndCreateSnapshot();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при створенні population snapshot");
            }

            // Перевіряємо раз на день
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task CheckAndCreateSnapshot()
    {
        var today = DateTime.Now;

        // Запускаємо тільки 31 грудня
        if (today.Month != 12 || today.Day != 31)
            return;

        using var scope = _serviceProvider.CreateScope();
        var personRepo = scope.ServiceProvider.GetRequiredService<IPersonRepository>();
        var snapshotRepo = scope.ServiceProvider.GetRequiredService<IPopulationSnapshotRepository>();

        var currentYear = today.Year;

        // Перевіряємо чи snapshot вже існує для цього року
        var existing = await snapshotRepo.SearchAsync(currentYear.ToString());
        if (existing.Any())
        {
            _logger.LogInformation($"Snapshot для {currentYear} року вже існує");
            return;
        }

        var population = await personRepo.GetPopulationByVillageAsync();

        foreach (var (village, count) in population)
        {
            await snapshotRepo.CreateAsync(new PopulationSnapshot
            {
                SettlementName = village,
                Year = currentYear,
                Population = count,
                CreatedAt = DateTime.Now
            });
        }

        _logger.LogInformation($"Population snapshot для {currentYear} року створено успішно");
    }
}
