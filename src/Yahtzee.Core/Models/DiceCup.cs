using Yahtzee.Core.Services;

namespace Yahtzee.Core.Models;

/// <summary>
/// Manages a set of 5 dice and tracks roll limits per turn.
/// </summary>
public class DiceCup
{
    /// <summary>
    /// The fixed number of dice in a cup (5).
    /// </summary>
    public const int DiceCount = 5;

    /// <summary>
    /// The maximum number of rolls allowed per round (3).
    /// </summary>
    public const int MaxRollsPerRound = 3;

    private readonly Die[] _dice;
    private readonly IRandomProvider _randomProvider;

    /// <summary>
    /// Gets the read-only list of 5 dice.
    /// </summary>
    public IReadOnlyList<Die> Dice => _dice;

    /// <summary>
    /// Gets the number of rolls remaining in the current turn.
    /// </summary>
    public int RollsRemaining { get; private set; } = MaxRollsPerRound;

    /// <summary>
    /// Gets a value indicating whether at least one roll has occurred in the current turn.
    /// </summary>
    public bool HasRolledThisRound => RollsRemaining < MaxRollsPerRound;

    /// <summary>
    /// Gets a value indicating whether rolls are still available in the current turn.
    /// </summary>
    public bool CanRoll => RollsRemaining > 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiceCup"/> class.
    /// </summary>
    /// <param name="randomProvider">Optional custom random provider.</param>
    public DiceCup(IRandomProvider? randomProvider = null)
    {
        _randomProvider = randomProvider ?? new RandomProvider();
        _dice = new Die[DiceCount];
        for (int i = 0; i < DiceCount; i++)
        {
            _dice[i] = new Die();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiceCup"/> class with pre-set values.
    /// </summary>
    /// <param name="initialValues">The 5 initial values for the dice.</param>
    /// <param name="rollsRemaining">Number of rolls remaining.</param>
    /// <param name="randomProvider">Optional custom random provider.</param>
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
