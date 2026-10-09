using Yahtzee.Core.Scoring;

namespace Yahtzee.Core.AI;

/// <summary>
/// Implements a simple heuristic-based AI strategy for Yahtzee.
/// </summary>
public class BasicAiStrategy : IAiStrategy
{
    /// <inheritdoc />
    public string Name => "Basic AI";

    /// <inheritdoc />
    public ISet<int> ChooseDiceToHold(IReadOnlyList<int> diceValues, Scorecard scorecard, int rollsRemaining)
    {
        var holds = new HashSet<int>();
        if (diceValues.Count != 5) return holds;

        var mostCommon = diceValues
            .GroupBy(v => v)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .FirstOrDefault();

        if (mostCommon != null && mostCommon.Count() >= 2)
        {
            int targetValue = mostCommon.Key;
            for (int i = 0; i < diceValues.Count; i++)
            {
                if (diceValues[i] == targetValue)
                {
                    holds.Add(i);
                }
            }
        }

        return holds;
    }

    /// <inheritdoc />
    public ScoreCategory ChooseCategoryToFill(IReadOnlyList<int> diceValues, Scorecard scorecard)
    {
        var previews = ScoreCalculator.PreviewAllScores(diceValues);

        var bestAvailable = previews
            .Where(kv => !scorecard.IsCategoryFilled(kv.Key))
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => (int)kv.Key)
            .FirstOrDefault();

        return bestAvailable.Key;
    }
}
