using Yahtzee.Core.Services;

namespace Yahtzee.Core.Models;

/// <summary>
/// Represents a single 6-sided die in the Yahtzee game.
/// </summary>
public class Die
{
    /// <summary>
    /// Gets the current face value of the die (1 to 6).
    /// </summary>
    public int Value { get; private set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the die is held (saved from being re-rolled).
    /// </summary>
    public bool IsHeld { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Die"/> class with default value 1.
    /// </summary>
    public Die() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Die"/> class with a specified initial value and hold status.
    /// </summary>
    /// <param name="initialValue">Initial face value (1-6).</param>
    /// <param name="isHeld">Initial held status.</param>
    public Die(int initialValue, bool isHeld = false)
    {
        if (initialValue is < 1 or > 6)
            throw new ArgumentOutOfRangeException(nameof(initialValue), "Die value must be between 1 and 6.");

        Value = initialValue;
        IsHeld = isHeld;
    }

    /// <summary>
    /// Rolls the die.
    /// </summary>
    /// <param name="randomProvider">The random number provider.</param>
    public void Roll(IRandomProvider randomProvider)
    {
        ArgumentNullException.ThrowIfNull(randomProvider);

        if (!IsHeld)
        {
            Value = randomProvider.NextDieValue();
        }
    }

    /// <summary>
    /// Resets the die.
    /// </summary>
    public void Reset()
    {
        IsHeld = false;
    }
}
