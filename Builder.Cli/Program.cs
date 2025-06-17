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
using Npgsql;

namespace Builder.Cli
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var host = CreateHostBuilder(args).Build();
            NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {

                    var dbContext = services.GetRequiredService<StatisticsDbContext>();

                    await dbContext.Database.MigrateAsync();

                    var configuration = services.GetRequiredService<IConfiguration>();
                    var apiKey = configuration["ApiKeys:RiotApiKey"];


                    var dataService = services.GetRequiredService<DataService>();
                    var matchIDRequestService = services.GetRequiredService<MatchIDRequestService>();
                    var matchDataRequestService = services.GetRequiredService<MatchDataRequestService>();

                    await BuildDatabase(dataService, configuration, matchDataRequestService, matchIDRequestService);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"An error occurred during application execution: {ex.Message}");
                    Console.Error.WriteLine(ex.StackTrace); 
                }
            }
        }

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
            await addMatchesToQueue(initialPUUID, matchIDRequestService, matchBFSQueue, visitedMatchIds);
            int gamesChecked = 0;
            int maxSize = 10000;
            while (gamesChecked < maxSize)
            {
                string currentMatchId = matchBFSQueue.Dequeue();
                Match currentMatch = await matchDataRequestService.GetMatchData(currentMatchId) ?? throw new Exception($"Could not get match data for match Id {currentMatchId}");
                if (currentMatch.info.tft_game_type != "standard" || currentMatch.info.tft_set_number != 14 || currentMatch.info.queue_id != 1100)
                {
                    Console.WriteLine("FOUND A NON-RANKED GAME");
                    continue;
                }
                await dataService.AddMatch(currentMatch);

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
                    await addMatchesToQueue(puuid, matchIDRequestService, matchBFSQueue, visitedMatchIds);
                }
                await Task.Delay(5000);
                gamesChecked++;
            }
        }

        private async static Task addMatchesToQueue(
            string newPUUID,
            MatchIDRequestService matchIDRequestService,
            Queue<string> matchBFSQueue,
            HashSet<string> visitedMatchIds)
        {
            if (matchBFSQueue.Count > 100)
            {
                return;
            }
            List<string> newMatchIds = await matchIDRequestService.getMatchIDs(newPUUID);
            foreach (string matchId in newMatchIds)
            {
                if (!visitedMatchIds.Contains(matchId))
                {
                    matchBFSQueue.Enqueue(matchId);
                    visitedMatchIds.Add(matchId);
                }
            }
            return;
        }
    }
}