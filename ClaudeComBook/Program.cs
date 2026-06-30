using ClaudeComBook.API.Data;
using ClaudeComBook.API.Repositories;
using ClaudeComBook.API.Repositories.Interfaces;
using ClaudeComBook.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DbConnectionFactory>();

builder.Services.AddScoped<IVillageRepository, VillageRepository>();
builder.Services.AddScoped<IStreetRepository, StreetRepository>();
builder.Services.AddScoped<IVillageStreetRepository, VillageStreetRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IHouseRepository, HouseRepository>();
builder.Services.AddScoped<IAnymalRepository, AnymalRepository>();
builder.Services.AddScoped<IEnterpriseRepository, EnterpriseRepository>();
builder.Services.AddScoped<IPlotRepository, PlotRepository>();
builder.Services.AddScoped<IPopulationSnapshotRepository, PopulationSnapshotRepository>(); builder.Services.AddHostedService<PopulationSnapshotService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();