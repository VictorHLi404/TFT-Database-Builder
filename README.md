This is an open-source tool used for generating TFT statistics databases in PostgreSQL using Entity Framework Core and ASP.NET 9.0. Currently, it is being used to generate statictics for Four-Two, a publically available TFT practice tool which calls on a private instance of a database generated using this tool via AWS Lambda.

Currently, the tool primarily focuses on generating winrate placements for teams and individual champions dependent on items and tier. It supports fuzzy matching of teams (e.g one champion replaced and/or removed/added), which allows for richer insights to be made by GIN indexing.

To run the application sucessfully, it is necessary to configure the following user secrets in ASP.NET INSIDE of the Builder.Cli project:

1. A valid database connection string at [connectionStrings:DefaultConnection]
2. A valid Riot Games API token at [ApiKeys:RiotApiKey]
3. An initial PUUID (player id) at [ApiKeys:InitialPUUID]. Please read the official [Riot Developer Portal](https://developer.riotgames.com/apis) for more information on how to get an ID.

A database MUST be first initialized by another means. This tool is meant to connect to an existing database created via PgAdmin or some other tool.

The tool works by combining the /tft/match/v1/matches/by-puuid/{puuid}/ids endpoint that provides match IDs with /tft/match/v1/matches/{matchId} that provides actual match data, including the PUUIDs of players. A BFS queue is then created of all of the match ids and processed accordingly.

