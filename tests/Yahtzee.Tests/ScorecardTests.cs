using Yahtzee.Core.Scoring;

namespace Yahtzee.Tests;

public class ScorecardTests
{
    [Fact]
    public void Scorecard_InitialState_AllCategoriesUnfilled()
    {
        var scorecard = new Scorecard();

        Assert.False(scorecard.IsComplete);
        Assert.Equal(0, scorecard.FilledCategoryCount);
        Assert.Equal(0, scorecard.TotalScore);
        Assert.All(Enum.GetValues<ScoreCategory>(), c => Assert.False(scorecard.IsCategoryFilled(c)));
    }

    [Fact]
    public void RecordScore_FillsCategory_AndUpdatesTotal()
    {
        var scorecard = new Scorecard();
        int[] dice = [5, 5, 5, 2, 1];

        bool result = scorecard.RecordScore(ScoreCategory.Fives, dice);

        Assert.True(result);
        Assert.True(scorecard.IsCategoryFilled(ScoreCategory.Fives));
        Assert.Equal(15, scorecard.GetScore(ScoreCategory.Fives));
        Assert.Equal(15, scorecard.UpperSectionSubtotal);
        Assert.Equal(0, scorecard.UpperSectionBonus);
        Assert.Equal(15, scorecard.TotalScore);
    }

    [Fact]
    public void RecordScore_AlreadyFilled_ReturnsFalse()
    {
        var scorecard = new Scorecard();
        int[] dice = [5, 5, 5, 2, 1];

        scorecard.RecordScore(ScoreCategory.Fives, dice);
        bool secondAttempt = scorecard.RecordScore(ScoreCategory.Fives, dice);

        Assert.False(secondAttempt);
    }

    [Fact]
    public void UpperSectionBonus_AwardedWhenSubtotalReaches63()
    {
        var scorecard = new Scorecard();

        // 3*3 = 9
        scorecard.RecordScore(ScoreCategory.Threes, [3, 3, 3, 1, 1]);
        // 4*4 = 16
        scorecard.RecordScore(ScoreCategory.Fours, [4, 4, 4, 4, 1]);
        // 5*4 = 20
        scorecard.RecordScore(ScoreCategory.Fives, [5, 5, 5, 5, 1]);
        // 6*3 = 18
        scorecard.RecordScore(ScoreCategory.Sixes, [6, 6, 6, 1, 1]);

        // Subtotal = 9 + 16 + 20 + 18 = 63 -> Bonus = 35!
        Assert.Equal(63, scorecard.UpperSectionSubtotal);
        Assert.Equal(35, scorecard.UpperSectionBonus);
        Assert.Equal(98, scorecard.TotalScore);
    }

    [Fact]
    public void IsComplete_TrueOnlyWhenAll13CategoriesFilled()
    {
        var scorecard = new Scorecard();
        int[] dice = [1, 2, 3, 4, 5];

        foreach (ScoreCategory category in Enum.GetValues<ScoreCategory>())
        {
            Assert.False(scorecard.IsComplete);
            scorecard.RecordScore(category, dice);
        }

        Assert.True(scorecard.IsComplete);
        Assert.Equal(13, scorecard.FilledCategoryCount);
    }
}
