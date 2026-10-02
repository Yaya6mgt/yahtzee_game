using Yahtzee.Core.AI;
using Yahtzee.Core.Database;
using Yahtzee.Core.Models;
using Yahtzee.Core.Persistence;
using Yahtzee.Core.Scoring;
using Yahtzee.Core.Services;

namespace Yahtzee.Core.Game;

public class GameSession
{
    public const int TotalRounds = 13;

    private readonly List<Player> _players;
    private readonly IGameDatabaseRepository? _database;
    private readonly IRandomProvider _randomProvider;

    public IReadOnlyList<Player> Players => _players;
    public int CurrentPlayerIndex { get; private set; } = 0;
    public int CurrentRound { get; private set; } = 1;
    public DiceCup DiceCup { get; }
    public bool HasUnsavedChanges { get; set; }

    public Player CurrentPlayer => _players[CurrentPlayerIndex];
    public bool IsGameOver => CurrentRound > TotalRounds || _players.All(p => p.Scorecard.IsComplete);

    public event EventHandler? TurnChanged;
    public event EventHandler? GameOver;

    public GameSession(
        IEnumerable<Player> players,
        IGameDatabaseRepository? database = null,
        IRandomProvider? randomProvider = null)
    {
        var playerList = players.ToList();
        if (playerList.Count == 0)
            throw new ArgumentException("At least one player is required to start a game.", nameof(players));

        _players = playerList;
        _database = database;
        _randomProvider = randomProvider ?? new RandomProvider();
        DiceCup = new DiceCup(_randomProvider);
    }

    public bool RollDice()
    {
        if (IsGameOver) return false;

        bool success = DiceCup.Roll();
        if (success)
        {
            HasUnsavedChanges = true;
        }
        return success;
    }

    public bool ToggleHold(int dieIndex)
    {
        if (IsGameOver) return false;
        bool success = DiceCup.ToggleHold(dieIndex);
        if (success)
        {
            HasUnsavedChanges = true;
        }
        return success;
    }

    public bool SelectCategory(ScoreCategory category)
    {
        if (IsGameOver) return false;
        if (!DiceCup.HasRolledThisRound) return false;

        if (CurrentPlayer.Scorecard.IsCategoryFilled(category))
            return false;

        bool recorded = CurrentPlayer.Scorecard.RecordScore(category, DiceCup.GetValues());
        if (!recorded) return false;

        HasUnsavedChanges = true;
        AdvanceTurn();
        return true;
    }

    public async Task ExecuteAiTurnStepAsync(Action? onStepCallback = null)
    {
        if (IsGameOver || !CurrentPlayer.IsAi) return;

        IAiStrategy strategy = CurrentPlayer is AiPlayer ai && ai.StrategyName == "Smart"
            ? new SmartAiStrategy()
            : new BasicAiStrategy();

        while (DiceCup.CanRoll)
        {
            RollDice();
            onStepCallback?.Invoke();

            if (DiceCup.CanRoll)
            {
                var holds = strategy.ChooseDiceToHold(DiceCup.GetValues(), CurrentPlayer.Scorecard, DiceCup.RollsRemaining);
                for (int i = 0; i < DiceCup.DiceCount; i++)
                {
                    DiceCup.SetHold(i, holds.Contains(i));
                }
            }
        }

        var chosenCategory = strategy.ChooseCategoryToFill(DiceCup.GetValues(), CurrentPlayer.Scorecard);
        SelectCategory(chosenCategory);
        onStepCallback?.Invoke();

        await Task.CompletedTask;
    }

    private void AdvanceTurn()
    {
        DiceCup.ResetForNewRound();

        CurrentPlayerIndex++;
        if (CurrentPlayerIndex >= _players.Count)
        {
            CurrentPlayerIndex = 0;
            CurrentRound++;
        }

        if (IsGameOver)
        {
            _ = RecordGameOverAsync();
            GameOver?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            TurnChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public List<Player> GetRankedPlayers()
    {
        return _players.OrderByDescending(p => p.Scorecard.TotalScore).ToList();
    }

    private async Task RecordGameOverAsync()
    {
        if (_database == null) return;
        var scores = _players.Select(p => (p.Name, p.Scorecard.TotalScore, p.IsAi));
        await _database.RecordCompletedGameAsync(DateTime.UtcNow, scores);
    }

    public GameSaveData ToSaveData()
    {
        return new GameSaveData
        {
            SavedAt = DateTime.UtcNow,
            CurrentRound = CurrentRound,
            CurrentPlayerIndex = CurrentPlayerIndex,
            RollsRemaining = DiceCup.RollsRemaining,
            DiceValues = DiceCup.GetValues().ToList(),
            DiceHolds = DiceCup.Dice.Select(d => d.IsHeld).ToList(),
            Players = _players.Select(p => new PlayerSaveData
            {
                Name = p.Name,
                IsAi = p.IsAi,
                StrategyName = p is AiPlayer ai ? ai.StrategyName : null,
                Scores = p.Scorecard.Scores.ToDictionary(kv => kv.Key, kv => kv.Value)
            }).ToList()
        };
    }

    public static GameSession RestoreFromSaveData(GameSaveData saveData, IGameDatabaseRepository? database = null, IRandomProvider? randomProvider = null)
    {
        ArgumentNullException.ThrowIfNull(saveData);

        var players = new List<Player>();
        foreach (var pData in saveData.Players)
        {
            Player player = pData.IsAi
                ? new AiPlayer(pData.Name, pData.StrategyName ?? "Basic")
                : new HumanPlayer(pData.Name);

            foreach (var (cat, score) in pData.Scores)
            {
                player.Scorecard.SetCategoryScore(cat, score);
            }

            players.Add(player);
        }

        var session = new GameSession(players, database, randomProvider)
        {
            CurrentRound = saveData.CurrentRound,
            CurrentPlayerIndex = saveData.CurrentPlayerIndex
        };

        if (saveData.DiceValues.Count == DiceCup.DiceCount)
        {
            for (int i = 0; i < DiceCup.DiceCount; i++)
            {
                session.DiceCup.Dice[i].Roll(new FixedValueRandomProvider(saveData.DiceValues[i]));
                session.DiceCup.Dice[i].IsHeld = saveData.DiceHolds.ElementAtOrDefault(i);
            }
        }

        return session;
    }

    private class FixedValueRandomProvider : IRandomProvider
    {
        private readonly int _val;
        public FixedValueRandomProvider(int val) => _val = val;
        public int NextDieValue() => _val;
    }
}
