using Yahtzee.Core.Scoring;

namespace Yahtzee.Core.Persistence;

// DTO
public class GameSaveData
{
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public int CurrentRound { get; set; } = 1;
    public int CurrentPlayerIndex { get; set; } = 0;
    public int RollsRemaining { get; set; } = 3;
    public List<int> DiceValues { get; set; } = new();
    public List<bool> DiceHolds { get; set; } = new();
    public List<PlayerSaveData> Players { get; set; } = new();
}

public class PlayerSaveData
{
    public string Name { get; set; } = string.Empty;
    public bool IsAi { get; set; }
    public string? StrategyName { get; set; }
    public Dictionary<ScoreCategory, int?> Scores { get; set; } = new();
}
