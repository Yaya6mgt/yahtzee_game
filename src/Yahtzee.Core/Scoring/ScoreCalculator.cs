namespace Yahtzee.Core.Scoring;

public static class ScoreCalculator
{
    /// <summary>
    /// Calculates the score for a given score category and dice values.
    /// </summary>
    /// <param name="category">The score category.</param>
    /// <param name="diceValues">The values of the dice.</param>
    /// <returns>The score for the given category and dice values.</returns>
    public static int CalculateScore(ScoreCategory category, IEnumerable<int> diceValues)
    {
        ArgumentNullException.ThrowIfNull(diceValues);
        var dice = diceValues.ToList();

        if (dice.Count != 5)
            throw new ArgumentException("Must provide exactly 5 dice values.", nameof(diceValues));

        return category switch
        {
            ScoreCategory.Aces => SumMatchingDice(dice, 1),
            ScoreCategory.Twos => SumMatchingDice(dice, 2),
            ScoreCategory.Threes => SumMatchingDice(dice, 3),
            ScoreCategory.Fours => SumMatchingDice(dice, 4),
            ScoreCategory.Fives => SumMatchingDice(dice, 5),
            ScoreCategory.Sixes => SumMatchingDice(dice, 6),

            ScoreCategory.ThreeOfAKind => HasNOfAKind(dice, 3) ? dice.Sum() : 0,
            ScoreCategory.FourOfAKind => HasNOfAKind(dice, 4) ? dice.Sum() : 0,
            ScoreCategory.FullHouse => IsFullHouse(dice) ? 25 : 0,
            ScoreCategory.SmallStraight => IsSmallStraight(dice) ? 30 : 0,
            ScoreCategory.LargeStraight => IsLargeStraight(dice) ? 40 : 0,
            ScoreCategory.Yahtzee => IsYahtzee(dice) ? 50 : 0,
            ScoreCategory.Chance => dice.Sum(),

            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown score category.")
        };
    }

    /// <summary>
    /// Calculates the score for all score categories for the given dice values.
    /// </summary>
    /// <param name="diceValues">The values of the dice.</param>
    /// <returns>A dictionary mapping each score category to its score.</returns>
    public static Dictionary<ScoreCategory, int> PreviewAllScores(IEnumerable<int> diceValues)
    {
        var dice = diceValues.ToList();
        var preview = new Dictionary<ScoreCategory, int>();

        foreach (ScoreCategory category in Enum.GetValues<ScoreCategory>())
        {
            preview[category] = CalculateScore(category, dice);
        }

        return preview;
    }

    /// <summary>
    /// Calculates the sum of matching dice.
    /// </summary>
    /// <param name="dice">The values of the dice.</param>
    /// <param name="targetValue">The value to match.</param>
    /// <returns>The sum of matching dice.</returns>
    private static int SumMatchingDice(List<int> dice, int targetValue)
    {
        return dice.Where(d => d == targetValue).Sum();
    }

    /// <summary>
    /// Checks if the dice contain at least n of a kind.
    /// </summary>
    /// <param name="dice">The values of the dice.</param>
    /// <param name="n">The number of dice to check for.</param>
    /// <returns>True if the dice contain at least n of a kind, false otherwise.</returns>
    private static bool HasNOfAKind(List<int> dice, int n)
    {
        var counts = dice.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
        return counts.Values.Any(c => c >= n);
    }

    /// <summary>
    /// Checks if the dice form a full house.
    /// </summary>
    /// <param name="dice">The values of the dice.</param>
    /// <returns>True if the dice form a full house, false otherwise.</returns>
    private static bool IsFullHouse(List<int> dice)
    {
        var counts = dice.GroupBy(d => d).Select(g => g.Count()).ToList();
        return counts.Count == 2 && ((counts[0] == 3 && counts[1] == 2) || (counts[0] == 2 && counts[1] == 3));
    }

    /// <summary>
    /// Checks if the dice form a small straight.
    /// </summary>
    /// <param name="dice">The values of the dice.</param>
    /// <returns>True if the dice form a small straight, false otherwise.</returns>
    private static bool IsSmallStraight(List<int> dice)
    {
        var unique = dice.Distinct().OrderBy(v => v).ToList();
        if (unique.Count < 4) return false;

        bool hasSeq1 = unique.Contains(1) && unique.Contains(2) && unique.Contains(3) && unique.Contains(4);
        bool hasSeq2 = unique.Contains(2) && unique.Contains(3) && unique.Contains(4) && unique.Contains(5);
        bool hasSeq3 = unique.Contains(3) && unique.Contains(4) && unique.Contains(5) && unique.Contains(6);

        return hasSeq1 || hasSeq2 || hasSeq3;
    }

    /// <summary>
    /// Checks if the dice form a large straight.
    /// </summary>
    /// <param name="dice">The values of the dice.</param>
    /// <returns>True if the dice form a large straight, false otherwise.</returns>
    private static bool IsLargeStraight(List<int> dice)
    {
        var unique = dice.Distinct().OrderBy(v => v).ToList();
        if (unique.Count < 5) return false;

        bool hasSeq1 = unique.SequenceEqual([1, 2, 3, 4, 5]);
        bool hasSeq2 = unique.SequenceEqual([2, 3, 4, 5, 6]);

        return hasSeq1 || hasSeq2;
    }

    /// <summary>
    /// Checks if the dice form a Yahtzee.
    /// </summary>
    /// <param name="dice">The values of the dice.</param>
    /// <returns>True if the dice form a Yahtzee, false otherwise.</returns>
    private static bool IsYahtzee(List<int> dice)
    {
        return dice.Distinct().Count() == 1;
    }
}
