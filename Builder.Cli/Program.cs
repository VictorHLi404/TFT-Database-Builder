using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Builder.Data; // Ensure this namespace matches where StatisticsDbContext resides
using System;
using System.Threading.Tasks;
using Builder.Cli.Services;
using System.Runtime.CompilerServices;
using Builder.Cli.Dtos;

namespace Builder.Cli
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // 1. Build the Host: This sets up your application's configuration and services.
            var host = CreateHostBuilder(args).Build();

            // 2. Create a Service Scope: DbContexts (and other scoped services) need a scope
            //    to exist within. For console apps, you explicitly create one.
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // 3. Get your DbContext instance from the service provider.
                    var dbContext = services.GetRequiredService<StatisticsDbContext>();

                    // Optional: Apply pending database migrations on application startup.
                    // This is common in development/testing environments but can be separated
                    // in production for more control.
                    Console.WriteLine("Applying database migrations...");
                    await dbContext.Database.MigrateAsync();
                    Console.WriteLine("Database migrations applied successfully!");


                    var configuration = services.GetRequiredService<IConfiguration>();
                    var apiKey = configuration["ApiKeys:RiotApiKey"];
                    Console.WriteLine($"Riot API Key (from configuration): {apiKey}"); // Moved here!

                    // 4. Run your main application logic.

                    var dataService = services.GetRequiredService<DataService>();
                    var matchIDRequestService = services.GetRequiredService<MatchIDRequestService>();
                    var matchDataRequestService = services.GetRequiredService<MatchDataRequestService>();

                    //    You can get other registered services here (e.g., your TeamCompService).
                    // var teamCompService = services.GetRequiredService<TeamCompService>();
                    // await RunApplicationLogic(teamCompService, dbContext);
                    await BuildDatabase(dataService, configuration, matchDataRequestService, matchIDRequestService);
                    Console.WriteLine("Console application finished.");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occurred during application execution: {ex.Message}");
                    Console.Error.WriteLine(ex.StackTrace); // Print stack trace for debugging
                }
            }
        }

        // --- Host Configuration Method ---
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    configuration.AddEnvironmentVariables();
                    if (args != null)
                    {
                        configuration.AddCommandLine(args);
                    }
                    configuration.AddUserSecrets<Program>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<StatisticsDbContext>(options =>
                    {
                        var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");

                        if (string.IsNullOrEmpty(connectionString))
                        {
                            throw new InvalidOperationException(
                                "Connection string 'DefaultConnection' not found in appsettings.json or is empty.");
                        }

                        options.UseNpgsql(connectionString);

                        options.EnableSensitiveDataLogging();
                    });
                    services.AddHttpClient<MatchDataRequestService>();
                    services.AddHttpClient<MatchIDRequestService>();
                    services.AddScoped<DataService>();

                    // Add any other services your console app needs.
                });

        private static async Task BuildDatabase(
            DataService dataService,
            IConfiguration configuration,
            MatchDataRequestService matchDataRequestService,
            MatchIDRequestService matchIDRequestService)
        {
            var initialPUUID = configuration["ApiKeys:InitialPUUID"];
            if (initialPUUID == null)
            {
                throw new Exception("Could not find initial PUUID.");
            }
            HashSet<string> visitedMatchIds = new HashSet<string>();
            Queue<string> matchBFSQueue = new Queue<string>();

            List<string> initialMatches = await matchIDRequestService.getMatchIDs(initialPUUID);
            addMatchesToQueue(initialMatches, matchBFSQueue, visitedMatchIds);
            int gamesChecked = 0;
            int maxSize = 20000;
            while (gamesChecked < maxSize)
            {
                string currentMatchId = matchBFSQueue.Dequeue();
                Match currentMatch = await matchDataRequestService.GetMatchData(currentMatchId) ?? throw new Exception($"Could not get match data for match Id {currentMatchId}");
                foreach (Participant participant in currentMatch.info.participants)
                {
                    foreach (Unit unit in participant.units)
                    {
                        await dataService.AddChampionEntity(unit, participant.placement);
                    }
                }
                List<Participant> participants = currentMatch.info.participants;
                List<string> newPUUIDs = new List<string>();
                foreach (Participant participant in participants)
                {
                    if (participant.puuid != null)
                    {
                        newPUUIDs.Add(participant.puuid);
                    }
                }
                foreach (string puuid in newPUUIDs)
                {
                    Console.Write($"{puuid} ");
                }
                foreach (string puuid in newPUUIDs)
                {
                    List<string> newMatches = await matchIDRequestService.getMatchIDs(puuid);
                    addMatchesToQueue(newMatches, matchBFSQueue, visitedMatchIds);
                }
                Thread.Sleep(2000);
                gamesChecked++;
            }
        }

        private static void addMatchesToQueue(
            List<string> newMatchIds,
            Queue<string> matchBFSQueue,
            HashSet<string> visitedMatchIds)
        {
            foreach (string matchId in newMatchIds)
            {
                if (!visitedMatchIds.Contains(matchId))
                {
                    matchBFSQueue.Enqueue(matchId);
                    visitedMatchIds.Add(matchId);
                }
            }
        }
    }
}