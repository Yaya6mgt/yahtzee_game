namespace Yahtzee.Core.Services;

public interface IRandomProvider
{
    int NextDieValue();
}

public class RandomProvider : IRandomProvider
{
    /// <summary>
    /// Generates a random die value.
    /// </summary>
    /// <returns>A random die value between 1 and 6.</returns>
    public int NextDieValue() => Random.Shared.Next(1, 7);
}
