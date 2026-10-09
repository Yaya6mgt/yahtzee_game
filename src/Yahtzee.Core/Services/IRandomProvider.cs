namespace Yahtzee.Core.Services;

/// <summary>
/// Service interface providing pseudo-random values for dice rolling.
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    /// Generates a random integer value between 1 and 6 (inclusive).
    /// </summary>
    /// <returns>A random die face value from 1 to 6.</returns>
    int NextDieValue();
}

/// <summary>
/// Default implementation of <see cref="IRandomProvider"/> using System.Random.Shared.
/// </summary>
public class RandomProvider : IRandomProvider
{
    /// <summary>
    /// Generates a random die value.
    /// </summary>
    /// <returns>A random die value between 1 and 6.</returns>
    public int NextDieValue() => Random.Shared.Next(1, 7);
}
