using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Builder.Data;
using Npgsql;
using Builder.LambdaApi.Services;
using System.Text.Json.Serialization;
using Builder.LambdaApi.Helpers;

var builder = WebApplication.CreateBuilder(args);
NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddUserSecrets<Program>();

ConfigurationHelper.Initialize(builder.Configuration);

builder.Services.AddDbContext<StatisticsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' not found in appsettings.json or is empty.");
    }

    Console.WriteLine($"CHECK CONNECTION STRING: {connectionString}");

    options.UseNpgsql(connectionString);
    options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<BaseDataService>();
builder.Services.AddScoped<ChampionService>();
builder.Services.AddScoped<TeamService>();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
