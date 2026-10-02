namespace Yahtzee.Core.Scoring;

public static class ScoreCalculator
{
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

    private static int SumMatchingDice(List<int> dice, int targetValue)
    {
        return dice.Where(d => d == targetValue).Sum();
    }

    private static bool HasNOfAKind(List<int> dice, int n)
    {
        var counts = dice.GroupBy(d => d).ToDictionary(g => g.Key, g => g.Count());
        return counts.Values.Any(c => c >= n);
    }

    private static bool IsFullHouse(List<int> dice)
    {
        var counts = dice.GroupBy(d => d).Select(g => g.Count()).ToList();
        return counts.Count == 2 && ((counts[0] == 3 && counts[1] == 2) || (counts[0] == 2 && counts[1] == 3));
    }

    // Hardcoder, suffisant pour le moment (à refaire plus tard si possible)
    private static bool IsSmallStraight(List<int> dice)
    {
        var unique = dice.Distinct().OrderBy(v => v).ToList();
        if (unique.Count < 4) return false;

        bool hasSeq1 = unique.Contains(1) && unique.Contains(2) && unique.Contains(3) && unique.Contains(4);
        bool hasSeq2 = unique.Contains(2) && unique.Contains(3) && unique.Contains(4) && unique.Contains(5);
        bool hasSeq3 = unique.Contains(3) && unique.Contains(4) && unique.Contains(5) && unique.Contains(6);

        return hasSeq1 || hasSeq2 || hasSeq3;
    }

    // Hardcoder, suffisant pour le moment (à refaire plus tard si possible)
    private static bool IsLargeStraight(List<int> dice)
    {
        var unique = dice.Distinct().OrderBy(v => v).ToList();
        if (unique.Count < 5) return false;

        bool hasSeq1 = unique.SequenceEqual([1, 2, 3, 4, 5]);
        bool hasSeq2 = unique.SequenceEqual([2, 3, 4, 5, 6]);

        return hasSeq1 || hasSeq2;
    }

    private static bool IsYahtzee(List<int> dice)
    {
        return dice.Distinct().Count() == 1;
    }
}
