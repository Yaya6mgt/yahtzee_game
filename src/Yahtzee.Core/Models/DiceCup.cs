using Yahtzee.Core.Services;

namespace Yahtzee.Core.Models;

public class DiceCup
{
    public const int DiceCount = 5;
    public const int MaxRollsPerRound = 3;

    private readonly Die[] _dice;
    private readonly IRandomProvider _randomProvider;

    public IReadOnlyList<Die> Dice => _dice;
    public int RollsRemaining { get; private set; } = MaxRollsPerRound;
    public bool HasRolledThisRound => RollsRemaining < MaxRollsPerRound;
    public bool CanRoll => RollsRemaining > 0;

    public DiceCup(IRandomProvider? randomProvider = null)
    {
        _randomProvider = randomProvider ?? new RandomProvider();
        _dice = new Die[DiceCount];
        for (int i = 0; i < DiceCount; i++)
        {
            _dice[i] = new Die();
        }
    }

    public DiceCup(IEnumerable<int> initialValues, int rollsRemaining = MaxRollsPerRound, IRandomProvider? randomProvider = null)
    {
        _randomProvider = randomProvider ?? new RandomProvider();
        var list = initialValues.ToList();
        if (list.Count != DiceCount)
            throw new ArgumentException($"Must provide exactly {DiceCount} dice values.", nameof(initialValues));

        _dice = list.Select(v => new Die(v)).ToArray();
        RollsRemaining = rollsRemaining;
    }

    /// <summary>
    /// Rolls the dice.
    /// </summary>
    /// <returns>True if the dice were rolled successfully, false otherwise.</returns>
    public bool Roll()
    {
        if (!CanRoll) return false;

        foreach (var die in _dice)
        {
            die.Roll(_randomProvider);
        }

        RollsRemaining--;
        return true;
    }

    /// <summary>
    /// Toggles the hold state of a specific die.
    /// </summary>
    /// <param name="index">The index of the die to toggle.</param>
    /// <returns>True if the die was toggled successfully, false otherwise.</returns>
    public bool ToggleHold(int index)
    {
        if (index < 0 || index >= DiceCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (!HasRolledThisRound) return false;

        _dice[index].IsHeld = !_dice[index].IsHeld;
        return true;
    }

    /// <summary>
    /// Sets the hold state of a specific die.
    /// </summary>
    /// <param name="index">The index of the die to set.</param>
    /// <param name="isHeld">The hold state to set.</param>
    /// <returns>True if the die was set successfully, false otherwise.</returns>
    public bool SetHold(int index, bool isHeld)
    {
        if (index < 0 || index >= DiceCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (!HasRolledThisRound) return false;

        _dice[index].IsHeld = isHeld;
        return true;
    }

    /// <summary>
    /// Gets the values of all dice.
    /// </summary>
    /// <returns>An array of die values.</returns>
    public int[] GetValues() => _dice.Select(d => d.Value).ToArray();

    /// <summary>
    /// Resets the dice for a new round.
    /// </summary>
    public void ResetForNewRound()
    {
        foreach (var die in _dice)
        {
            die.Reset();
        }
        RollsRemaining = MaxRollsPerRound;
    }
}
