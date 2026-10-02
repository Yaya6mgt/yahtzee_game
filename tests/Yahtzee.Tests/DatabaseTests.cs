using Yahtzee.Core.Database;

namespace Yahtzee.Tests;

public class DatabaseTests
{
    [Fact]
    public async Task SqliteGameDatabase_RecordsGameAndQueriesTop5HighScores()
    {
        string dbPath = Path.Combine(Path.GetTempPath(), $"yahtzee_db_{Guid.NewGuid():N}.db");
        try
        {
            var db = new SqliteGameDatabase(dbPath);
            await db.InitializeDatabaseAsync();

            var game1Scores = new List<(string name, int score, bool isAi)>
            {
                ("Alice", 250, false),
                ("Bob", 180, false),
                ("Charlie", 310, false)
            };

            await db.RecordCompletedGameAsync(DateTime.UtcNow, game1Scores);

            var topOverall = await db.GetTop5HighScoresOverallAsync();
            Assert.NotEmpty(topOverall);
            Assert.Equal("Charlie", topOverall[0].PlayerName);
            Assert.Equal(310, topOverall[0].Score);

            var aliceScores = await db.GetTop5HighScoresForPlayerAsync("Alice");
            Assert.Single(aliceScores);
            Assert.Equal("Alice", aliceScores[0].PlayerName);
            Assert.Equal(250, aliceScores[0].Score);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
