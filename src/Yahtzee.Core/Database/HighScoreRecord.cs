namespace Yahtzee.Core.Database;

/// <summary>
/// Represents a high score record.
/// </summary>
public record HighScoreRecord(
    string PlayerName,
    int Score,
    DateTime PlayedAt,
    bool IsAi,
    string GameId
);

/// <summary>
/// Represents a completed game record.
/// </summary>
public record CompletedGameRecord(
    string GameId,
    DateTime PlayedAt,
    string WinnerName,
    int WinnerScore,
    int PlayerCount
);
