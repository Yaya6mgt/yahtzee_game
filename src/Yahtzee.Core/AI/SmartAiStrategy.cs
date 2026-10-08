using Yahtzee.Core.Scoring;

namespace Yahtzee.Core.AI;

public class SmartAiStrategy : IAiStrategy
{
    public string Name => "Smart AI";

    public ISet<int> ChooseDiceToHold(IReadOnlyList<int> diceValues, Scorecard scorecard, int rollsRemaining)
    {
        var holds = new HashSet<int>();
        if (diceValues.Count != 5) return holds;

        var previews = ScoreCalculator.PreviewAllScores(diceValues);

        if (!scorecard.IsCategoryFilled(ScoreCategory.Yahtzee) && previews[ScoreCategory.Yahtzee] == 50)
        {
            return new HashSet<int> { 0, 1, 2, 3, 4 };
        }

        if (!scorecard.IsCategoryFilled(ScoreCategory.LargeStraight) && previews[ScoreCategory.LargeStraight] == 40)
        {
            return new HashSet<int> { 0, 1, 2, 3, 4 };
        }

        if (!scorecard.IsCategoryFilled(ScoreCategory.FullHouse) && previews[ScoreCategory.FullHouse] == 25)
        {
            return new HashSet<int> { 0, 1, 2, 3, 4 };
        }

        var groups = diceValues.Select((val, idx) => (val, idx))
            .GroupBy(x => x.val)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .ToList();

        if (groups.Count > 0 && groups[0].Count() >= 3)
        {
            foreach (var item in groups[0])
            {
                holds.Add(item.idx);
            }
            return holds;
        }

        var uniqueIndices = diceValues
            .Select((val, idx) => (val, idx))
            .GroupBy(x => x.val)
            .Select(g => g.First())
            .OrderBy(x => x.val)
            .ToList();

        if (!scorecard.IsCategoryFilled(ScoreCategory.SmallStraight) || !scorecard.IsCategoryFilled(ScoreCategory.LargeStraight))
        {
            var straightIndices = FindStraightSequenceIndices(uniqueIndices);
            if (straightIndices.Count >= 3)
            {
                return straightIndices;
            }
        }

        if (groups.Count > 0 && groups[0].Count() >= 2)
        {
            foreach (var item in groups[0])
            {
                holds.Add(item.idx);
            }
            return holds;
        }

        var maxDie = diceValues.Select((val, idx) => (val, idx)).OrderByDescending(x => x.val).First();
        holds.Add(maxDie.idx);
        return holds;
    }

    public ScoreCategory ChooseCategoryToFill(IReadOnlyList<int> diceValues, Scorecard scorecard)
    {
        var previews = ScoreCalculator.PreviewAllScores(diceValues);
        var openCategories = Enum.GetValues<ScoreCategory>()
            .Where(c => !scorecard.IsCategoryFilled(c))
            .ToList();

        if (openCategories.Contains(ScoreCategory.Yahtzee) && previews[ScoreCategory.Yahtzee] == 50)
            return ScoreCategory.Yahtzee;

        if (openCategories.Contains(ScoreCategory.LargeStraight) && previews[ScoreCategory.LargeStraight] == 40)
            return ScoreCategory.LargeStraight;

        if (openCategories.Contains(ScoreCategory.SmallStraight) && previews[ScoreCategory.SmallStraight] == 30)
            return ScoreCategory.SmallStraight;

        if (openCategories.Contains(ScoreCategory.FullHouse) && previews[ScoreCategory.FullHouse] == 25)
            return ScoreCategory.FullHouse;

        if (openCategories.Contains(ScoreCategory.FourOfAKind) && previews[ScoreCategory.FourOfAKind] > 0)
            return ScoreCategory.FourOfAKind;

        if (openCategories.Contains(ScoreCategory.ThreeOfAKind) && previews[ScoreCategory.ThreeOfAKind] >= 18)
            return ScoreCategory.ThreeOfAKind;

        ScoreCategory[] categories = new ScoreCategory[]
        {
            ScoreCategory.Sixes,
            ScoreCategory.Fives,
            ScoreCategory.Fours,
            ScoreCategory.Threes,
            ScoreCategory.Twos,
            ScoreCategory.Aces
        };

        foreach (var category in categories)
        {
            if (openCategories.Contains(category))
            {
                int targetValue = (int)category;
                int count = diceValues.Count(d => d == targetValue);
                if (count >= 3)
                {
                    return category;
                }
            }
        }

        if (openCategories.Contains(ScoreCategory.Chance) && previews[ScoreCategory.Chance] >= 18)
            return ScoreCategory.Chance;

        foreach (var category in categories)
        {
            if (openCategories.Contains(category) && previews[category] > 0)
                return category;
        }

        ScoreCategory[] sacrificePriority = new ScoreCategory[]
        {
            ScoreCategory.Aces,
            ScoreCategory.Twos,
            ScoreCategory.ThreeOfAKind,
            ScoreCategory.FourOfAKind,
            ScoreCategory.FullHouse,
            ScoreCategory.SmallStraight,
            ScoreCategory.LargeStraight,
            ScoreCategory.Yahtzee,
            ScoreCategory.Chance
        };

        foreach (var category in sacrificePriority)
        {
            if (openCategories.Contains(category))
                return category;
        }

        return openCategories.First();
    }

    /// <summary>
    /// Finds the indices of the dice that form the longest straight sequence.
    /// </summary>
    /// <param name="uniqueDice">The unique dice values and their indices.</param>
    /// <returns>The indices of the dice that form the longest straight sequence.</returns>
    private static HashSet<int> FindStraightSequenceIndices(List<(int val, int idx)> uniqueDice)
    {
        var result = new HashSet<int>();
        for (int i = 0; i < uniqueDice.Count; i++)
        {
            var temp = new List<(int val, int idx)> { uniqueDice[i] };
            for (int j = i + 1; j < uniqueDice.Count; j++)
            {
                if (uniqueDice[j].val == temp.Last().val + 1)
                {
                    temp.Add(uniqueDice[j]);
                }
                else break;
            }
            if (temp.Count > result.Count)
            {
                result = temp.Select(t => t.idx).ToHashSet();
            }
        }
        return result;
    }
}
