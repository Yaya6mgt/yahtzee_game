using Yahtzee.Core.AI;
using Yahtzee.Core.Scoring;

namespace Yahtzee.Tests;

public class AiStrategyTests
{
    [Fact]
    public void BasicAiStrategy_HoldsMostCommonDiceValues()
    {
        var strategy = new BasicAiStrategy();
        var dice = new List<int> { 4, 4, 4, 2, 6 };
        var scorecard = new Scorecard();

        var holds = strategy.ChooseDiceToHold(dice, scorecard, 2);

        // Indices 0, 1, 2 have value 4
        Assert.Equal(new HashSet<int> { 0, 1, 2 }, holds);
    }

    [Fact]
    public void BasicAiStrategy_ChoosesHighestScoringOpenCategory()
    {
        var strategy = new BasicAiStrategy();
        var dice = new List<int> { 5, 5, 5, 5, 5 };
        var scorecard = new Scorecard();

        var chosen = strategy.ChooseCategoryToFill(dice, scorecard);

        // Yahtzee gives 50 points, which is highest
        Assert.Equal(ScoreCategory.Yahtzee, chosen);
    }

    [Fact]
    public void SmartAiStrategy_HoldsAllDiceWhenYahtzeeHit()
    {
        var strategy = new SmartAiStrategy();
        var dice = new List<int> { 6, 6, 6, 6, 6 };
        var scorecard = new Scorecard();

        var holds = strategy.ChooseDiceToHold(dice, scorecard, 2);

        Assert.Equal(new HashSet<int> { 0, 1, 2, 3, 4 }, holds);
    }

    [Fact]
    public void SmartAiStrategy_ChoosesYahtzeeCategoryWhenAvailable()
    {
        var strategy = new SmartAiStrategy();
        var dice = new List<int> { 6, 6, 6, 6, 6 };
        var scorecard = new Scorecard();

        var chosen = strategy.ChooseCategoryToFill(dice, scorecard);

        Assert.Equal(ScoreCategory.Yahtzee, chosen);
    }
}
