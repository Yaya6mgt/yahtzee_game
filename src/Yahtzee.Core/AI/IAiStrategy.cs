using Yahtzee.Core.Scoring;

namespace Yahtzee.Core.AI;

public interface IAiStrategy
{
    string Name { get; }

    ISet<int> ChooseDiceToHold(IReadOnlyList<int> diceValues, Scorecard scorecard, int rollsRemaining);

    ScoreCategory ChooseCategoryToFill(IReadOnlyList<int> diceValues, Scorecard scorecard);
}
