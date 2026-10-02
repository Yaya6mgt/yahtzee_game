using Microsoft.Data.Sqlite;

namespace Yahtzee.Core.Database;

public interface IGameDatabaseRepository
{
    Task InitializeDatabaseAsync();
    Task RecordCompletedGameAsync(DateTime playedAt, IEnumerable<(string name, int score, bool isAi)> playerScores);
    Task<List<HighScoreRecord>> GetTop5HighScoresOverallAsync();
    Task<List<HighScoreRecord>> GetTop5HighScoresForPlayerAsync(string playerName);
    Task<List<CompletedGameRecord>> GetRecentGamesAsync(int limit = 20);
}

public class SqliteGameDatabase : IGameDatabaseRepository
{
    private readonly string _connectionString;

    public SqliteGameDatabase(string dbPath = "yahtzee_games.db")
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        };
        _connectionString = builder.ConnectionString;
    }

    public async Task InitializeDatabaseAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string createTableSql = """
            CREATE TABLE IF NOT EXISTS CompletedGames (
                Id TEXT PRIMARY KEY,
                PlayedAt TEXT NOT NULL,
                PlayerCount INTEGER NOT NULL,
                WinnerName TEXT NOT NULL,
                WinnerScore INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS GameScores (
                Id TEXT PRIMARY KEY,
                GameId TEXT NOT NULL,
                PlayerName TEXT NOT NULL,
                Score INTEGER NOT NULL,
                IsAi INTEGER NOT NULL,
                PlayedAt TEXT NOT NULL,
                FOREIGN KEY(GameId) REFERENCES CompletedGames(Id)
            );

            CREATE INDEX IF NOT EXISTS IX_GameScores_Score ON GameScores(Score DESC);
            CREATE INDEX IF NOT EXISTS IX_GameScores_PlayerName ON GameScores(PlayerName);
            """;

        using var command = connection.CreateCommand();
        command.CommandText = createTableSql;
        await command.ExecuteNonQueryAsync();
    }

    public async Task RecordCompletedGameAsync(DateTime playedAt, IEnumerable<(string name, int score, bool isAi)> playerScores)
    {
        var list = playerScores.ToList();
        if (list.Count == 0) return;

        var winner = list.OrderByDescending(p => p.score).First();
        string gameId = Guid.NewGuid().ToString("N");
        string playedAtIso = playedAt.ToString("o");

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        string insertGameSql = """
            INSERT INTO CompletedGames (Id, PlayedAt, PlayerCount, WinnerName, WinnerScore)
            VALUES (@Id, @PlayedAt, @PlayerCount, @WinnerName, @WinnerScore);
            """;

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = insertGameSql;
            cmd.Parameters.AddWithValue("@Id", gameId);
            cmd.Parameters.AddWithValue("@PlayedAt", playedAtIso);
            cmd.Parameters.AddWithValue("@PlayerCount", list.Count);
            cmd.Parameters.AddWithValue("@WinnerName", winner.name);
            cmd.Parameters.AddWithValue("@WinnerScore", winner.score);
            await cmd.ExecuteNonQueryAsync();
        }

        string insertScoreSql = """
            INSERT INTO GameScores (Id, GameId, PlayerName, Score, IsAi, PlayedAt)
            VALUES (@Id, @GameId, @PlayerName, @Score, @IsAi, @PlayedAt);
            """;

        foreach (var player in list)
        {
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = insertScoreSql;
            cmd.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString("N"));
            cmd.Parameters.AddWithValue("@GameId", gameId);
            cmd.Parameters.AddWithValue("@PlayerName", player.name);
            cmd.Parameters.AddWithValue("@Score", player.score);
            cmd.Parameters.AddWithValue("@IsAi", player.isAi ? 1 : 0);
            cmd.Parameters.AddWithValue("@PlayedAt", playedAtIso);
            await cmd.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
    }

    public async Task<List<HighScoreRecord>> GetTop5HighScoresOverallAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string query = """
            SELECT PlayerName, Score, PlayedAt, IsAi, GameId
            FROM GameScores
            ORDER BY Score DESC, PlayedAt DESC
            LIMIT 5;
            """;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        using var reader = await cmd.ExecuteReaderAsync();

        var records = new List<HighScoreRecord>();
        while (await reader.ReadAsync())
        {
            records.Add(new HighScoreRecord(
                reader.GetString(0),
                reader.GetInt32(1),
                DateTime.Parse(reader.GetString(2)),
                reader.GetInt32(3) == 1,
                reader.GetString(4)
            ));
        }

        return records;
    }

    public async Task<List<HighScoreRecord>> GetTop5HighScoresForPlayerAsync(string playerName)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string query = """
            SELECT PlayerName, Score, PlayedAt, IsAi, GameId
            FROM GameScores
            WHERE LOWER(PlayerName) = LOWER(@PlayerName)
            ORDER BY Score DESC, PlayedAt DESC
            LIMIT 5;
            """;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        cmd.Parameters.AddWithValue("@PlayerName", playerName.Trim());
        using var reader = await cmd.ExecuteReaderAsync();

        var records = new List<HighScoreRecord>();
        while (await reader.ReadAsync())
        {
            records.Add(new HighScoreRecord(
                reader.GetString(0),
                reader.GetInt32(1),
                DateTime.Parse(reader.GetString(2)),
                reader.GetInt32(3) == 1,
                reader.GetString(4)
            ));
        }

        return records;
    }

    public async Task<List<CompletedGameRecord>> GetRecentGamesAsync(int limit = 20)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        string query = """
            SELECT Id, PlayedAt, WinnerName, WinnerScore, PlayerCount
            FROM CompletedGames
            ORDER BY PlayedAt DESC
            LIMIT @Limit;
            """;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        cmd.Parameters.AddWithValue("@Limit", limit);
        using var reader = await cmd.ExecuteReaderAsync();

        var records = new List<CompletedGameRecord>();
        while (await reader.ReadAsync())
        {
            records.Add(new CompletedGameRecord(
                reader.GetString(0),
                DateTime.Parse(reader.GetString(1)),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4)
            ));
        }

        return records;
    }
}
