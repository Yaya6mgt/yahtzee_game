namespace Yahtzee.Core.Scoring;

public class Scorecard
{
    public const int UpperSectionBonusThreshold = 63;
    public const int UpperSectionBonusValue = 35;

    private readonly Dictionary<ScoreCategory, int?> _scores;

    public IReadOnlyDictionary<ScoreCategory, int?> Scores => _scores;

    public Scorecard()
    {
        _scores = new Dictionary<ScoreCategory, int?>();
        foreach (ScoreCategory category in Enum.GetValues<ScoreCategory>())
        {
            _scores[category] = null;
        }
    }

    /// <summary>
    /// Checks if a category is filled.
    /// </summary>
    /// <param name="category">The category to check.</param>
    /// <returns>True if the category is filled, false otherwise.</returns>
    public bool IsCategoryFilled(ScoreCategory category) => _scores[category].HasValue;

    /// <summary>
    /// Records a score for a category.
    /// </summary>
    /// <param name="category">The category to record the score for.</param>
    /// <param name="diceValues">The values of the dice.</param>
    /// <returns>True if the score was recorded, false otherwise.</returns>
    public bool RecordScore(ScoreCategory category, IEnumerable<int> diceValues)
    {
        if (IsCategoryFilled(category)) return false;

        int score = ScoreCalculator.CalculateScore(category, diceValues);
        _scores[category] = score;
        return true;
    }

    /// <summary>
    /// Sets the score for a category.
    /// </summary>
    /// <param name="category">The category to set the score for.</param>
    /// <param name="score">The score to set.</param>
    public void SetCategoryScore(ScoreCategory category, int? score)
    {
        _scores[category] = score;
    }

    /// <summary>
    /// Gets the score for a category.
    /// </summary>
    /// <param name="category">The category to get the score for.</param>
    /// <returns>The score for the given category.</returns>
    public int? GetScore(ScoreCategory category) => _scores[category];

    /// <summary>
    /// Gets the upper section subtotal.
    /// </summary>
    /// <returns>The upper section subtotal.</returns>
    public int UpperSectionSubtotal
    {
        get
        {
            return _scores
                .Where(kv => (int)kv.Key <= 6 && kv.Value.HasValue)
                .Sum(kv => kv.Value!.Value);
        }
    }

    /// <summary>
    /// Gets the upper section bonus.
    /// </summary>
    /// <returns>The upper section bonus.</returns>
    public int UpperSectionBonus => UpperSectionSubtotal >= UpperSectionBonusThreshold ? UpperSectionBonusValue : 0;

    /// <summary>
    /// Gets the lower section total.
    /// </summary>
    /// <returns>The lower section total.</returns>
    public int LowerSectionTotal
    {
        get
        {
            return _scores
                .Where(kv => (int)kv.Key >= 7 && kv.Value.HasValue)
                .Sum(kv => kv.Value!.Value);
        }
    }

    /// <summary>
    /// Gets the total score.
    /// </summary>
    /// <returns>The total score.</returns>
    public int TotalScore => UpperSectionSubtotal + UpperSectionBonus + LowerSectionTotal;

    /// <summary>
    /// Checks if the scorecard is complete.
    /// </summary>
    /// <returns>True if the scorecard is complete, false otherwise.</returns>
    public bool IsComplete => _scores.Values.All(s => s.HasValue);

    /// <summary>
    /// Gets the number of filled categories.
    /// </summary>
    /// <returns>The number of filled categories.</returns>
    public int FilledCategoryCount => _scores.Values.Count(s => s.HasValue);
}
