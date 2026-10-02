using Yahtzee.Core.Game;
using Yahtzee.Core.Models;
using Yahtzee.Core.Persistence;
using Yahtzee.Core.Scoring;

namespace Yahtzee.Tests;

public class SaveLoadTests
{
    [Fact]
    public async Task JsonSaveRepository_SavesAndLoadsGameSessionState()
    {
        string tempFile = Path.Combine(Path.GetTempPath(), $"yahtzee_test_{Guid.NewGuid():N}.json");
        try
        {
            var p1 = new HumanPlayer("Alice");
            var p2 = new AiPlayer("BobBot", "Smart");
            p1.Scorecard.RecordScore(ScoreCategory.Aces, [1, 1, 1, 3, 4]);

            var session = new GameSession([p1, p2]);
            session.RollDice();

            var repo = new JsonSaveRepository();
            var saveData = session.ToSaveData();

            await repo.SaveGameAsync(saveData, tempFile);

            Assert.True(File.Exists(tempFile));

            var loadedData = await repo.LoadGameAsync(tempFile);
            var restoredSession = GameSession.RestoreFromSaveData(loadedData);

            Assert.Equal(2, restoredSession.Players.Count);
            Assert.Equal("Alice", restoredSession.Players[0].Name);
            Assert.False(restoredSession.Players[0].IsAi);
            Assert.Equal(3, restoredSession.Players[0].Scorecard.GetScore(ScoreCategory.Aces));
            Assert.Equal("BobBot", restoredSession.Players[1].Name);
            Assert.True(restoredSession.Players[1].IsAi);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
