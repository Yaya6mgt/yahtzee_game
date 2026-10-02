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

    public bool IsCategoryFilled(ScoreCategory category) => _scores[category].HasValue;

    public bool RecordScore(ScoreCategory category, IEnumerable<int> diceValues)
    {
        if (IsCategoryFilled(category)) return false;

        int score = ScoreCalculator.CalculateScore(category, diceValues);
        _scores[category] = score;
        return true;
    }

    public void SetCategoryScore(ScoreCategory category, int? score)
    {
        _scores[category] = score;
    }

    public int? GetScore(ScoreCategory category) => _scores[category];

    public int UpperSectionSubtotal
    {
        get
        {
            return _scores
                .Where(kv => (int)kv.Key <= 6 && kv.Value.HasValue)
                .Sum(kv => kv.Value!.Value);
        }
    }

    public int UpperSectionBonus => UpperSectionSubtotal >= UpperSectionBonusThreshold ? UpperSectionBonusValue : 0;

    public int LowerSectionTotal
    {
        get
        {
            return _scores
                .Where(kv => (int)kv.Key >= 7 && kv.Value.HasValue)
                .Sum(kv => kv.Value!.Value);
        }
    }

    public int TotalScore => UpperSectionSubtotal + UpperSectionBonus + LowerSectionTotal;

    public bool IsComplete => _scores.Values.All(s => s.HasValue);

    public int FilledCategoryCount => _scores.Values.Count(s => s.HasValue);
}
