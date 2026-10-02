namespace Yahtzee.Core.Database;

public record HighScoreRecord(
    string PlayerName,
    int Score,
    DateTime PlayedAt,
    bool IsAi,
    string GameId
);

public record CompletedGameRecord(
    string GameId,
    DateTime PlayedAt,
    string WinnerName,
    int WinnerScore,
    int PlayerCount
);
