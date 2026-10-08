using Yahtzee.Core.Services;

namespace Yahtzee.Core.Models;

public class Die
{
    public int Value { get; private set; } = 1;
    public bool IsHeld { get; set; }

    public Die() { }

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
