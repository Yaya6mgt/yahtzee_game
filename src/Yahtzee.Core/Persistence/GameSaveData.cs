using Yahtzee.Core.Scoring;

namespace Yahtzee.Core.Persistence;

/// <summary>
/// Data transfer object for saving game state.
/// </summary>
public class GameSaveData
{
    /// <summary>
    /// The time the game was saved.
    /// </summary>
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The current round of the game.
    /// </summary>
    public int CurrentRound { get; set; } = 1;

    /// <summary>
    /// The index of the current player.
    /// </summary>
    public int CurrentPlayerIndex { get; set; } = 0;

    /// <summary>
    /// The number of rolls remaining.
    /// </summary>
    public int RollsRemaining { get; set; } = 3;

    /// <summary>
    /// The values of the dice.
    /// </summary>
    public List<int> DiceValues { get; set; } = new();

    /// <summary>
    /// The hold state of the dice.
    /// </summary>
    public List<bool> DiceHolds { get; set; } = new();

    /// <summary>
    /// The players in the game.
    /// </summary>
    public List<PlayerSaveData> Players { get; set; } = new();
}

/// <summary>
/// Data transfer object for saving a player's scorecard state and details.
/// </summary>
public class PlayerSaveData
{
    /// <summary>
    /// The name of the player.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the player is an AI.
    /// </summary>
    public bool IsAi { get; set; }

    /// <summary>
    /// The name of the AI strategy.
    /// </summary>
    public string? StrategyName { get; set; }

    /// <summary>
    /// The scores for each category.
    /// </summary>
    public Dictionary<ScoreCategory, int?> Scores { get; set; } = new();
}
