using Yahtzee.Core.Scoring;

namespace Yahtzee.Tests;

public class ScoreCalculatorTests
{
    [Theory]
    [InlineData(ScoreCategory.Aces, new[] { 1, 1, 3, 4, 1 }, 3)]
    [InlineData(ScoreCategory.Twos, new[] { 2, 2, 2, 4, 5 }, 6)]
    [InlineData(ScoreCategory.Threes, new[] { 3, 3, 3, 3, 1 }, 12)]
    [InlineData(ScoreCategory.Fours, new[] { 1, 2, 3, 5, 6 }, 0)]
    [InlineData(ScoreCategory.Fives, new[] { 5, 5, 5, 5, 5 }, 25)]
    [InlineData(ScoreCategory.Sixes, new[] { 6, 6, 1, 2, 3 }, 12)]
    public void UpperSection_CalculatesSumOfTargetNumber(ScoreCategory category, int[] dice, int expectedScore)
    {
        int score = ScoreCalculator.CalculateScore(category, dice);
        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public void ThreeOfAKind_ValidCombo_ReturnsSumOfAllDice()
    {
        int[] dice = [4, 4, 4, 2, 5];
        int score = ScoreCalculator.CalculateScore(ScoreCategory.ThreeOfAKind, dice);
        Assert.Equal(19, score);
    }

    [Fact]
    public void ThreeOfAKind_InvalidCombo_ReturnsZero()
    {
        int[] dice = [4, 4, 1, 2, 5];
        int score = ScoreCalculator.CalculateScore(ScoreCategory.ThreeOfAKind, dice);
        Assert.Equal(0, score);
    }

    [Fact]
    public void FourOfAKind_ValidCombo_ReturnsSumOfAllDice()
    {
        int[] dice = [6, 6, 6, 6, 1];
        int score = ScoreCalculator.CalculateScore(ScoreCategory.FourOfAKind, dice);
        Assert.Equal(25, score);
    }

    [Fact]
    public void FullHouse_ThreeAndTwo_Returns25()
    {
        int[] dice = [2, 2, 2, 5, 5];
        int score = ScoreCalculator.CalculateScore(ScoreCategory.FullHouse, dice);
        Assert.Equal(25, score);
    }

    [Fact]
    public void FullHouse_FiveOfAKind_ReturnsZero_PerSpec()
    {
        // Per Appendix B spec: "Note that a valid yahtzee is not a valid full house"
        int[] dice = [5, 5, 5, 5, 5];
        int score = ScoreCalculator.CalculateScore(ScoreCategory.FullHouse, dice);
        Assert.Equal(0, score);
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 6 }, 30)]
    [InlineData(new[] { 2, 3, 4, 5, 2 }, 30)]
    [InlineData(new[] { 3, 4, 5, 6, 1 }, 30)]
    [InlineData(new[] { 1, 2, 4, 5, 6 }, 0)]
    public void SmallStraight_Validation(int[] dice, int expectedScore)
    {
        int score = ScoreCalculator.CalculateScore(ScoreCategory.SmallStraight, dice);
        Assert.Equal(expectedScore, score);
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 40)]
    [InlineData(new[] { 2, 3, 4, 5, 6 }, 40)]
    [InlineData(new[] { 1, 2, 3, 4, 6 }, 0)]
    public void LargeStraight_Validation(int[] dice, int expectedScore)
    {
        int score = ScoreCalculator.CalculateScore(ScoreCategory.LargeStraight, dice);
        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public void Yahtzee_AllSame_Returns50()
    {
        int[] dice = [3, 3, 3, 3, 3];
        int score = ScoreCalculator.CalculateScore(ScoreCategory.Yahtzee, dice);
        Assert.Equal(50, score);
    }

    [Fact]
    public void PreviewAllScores_ReturnsPreviewFor13Categories()
    {
        int[] dice = [1, 2, 3, 4, 5];
        var previews = ScoreCalculator.PreviewAllScores(dice);

        Assert.Equal(13, previews.Count);
        Assert.Equal(1, previews[ScoreCategory.Aces]);
        Assert.Equal(30, previews[ScoreCategory.SmallStraight]);
        Assert.Equal(40, previews[ScoreCategory.LargeStraight]);
        Assert.Equal(15, previews[ScoreCategory.Chance]);
    }
}
