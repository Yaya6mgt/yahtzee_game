namespace Yahtzee.Core.Services;

public interface IRandomProvider
{
    int NextDieValue();
}

public class RandomProvider : IRandomProvider
{
    public int NextDieValue() => Random.Shared.Next(1, 7);
}
