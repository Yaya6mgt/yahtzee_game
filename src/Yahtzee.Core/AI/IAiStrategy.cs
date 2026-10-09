using Yahtzee.Core.Scoring;

namespace Yahtzee.Core.AI;

/// <summary>
/// Defines the strategy interface for AI-controlled players.
/// </summary>
public interface IAiStrategy
{
    /// <summary>
    /// Gets the unique display name of the AI strategy.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Chooses which dice to hold for the next roll.
    /// </summary>
    /// <param name="diceValues">The current values of the 5 dice.</param>
    /// <param name="scorecard">The current state of the scorecard.</param>
    /// <param name="rollsRemaining">The number of rolls remaining in the current turn.</param>
    /// <returns>A set of indices (0-4) of the dice to keep.</returns>
    ISet<int> ChooseDiceToHold(IReadOnlyList<int> diceValues, Scorecard scorecard, int rollsRemaining);

    /// <summary>
    /// Chooses which category to fill on the scorecard.
    /// </summary>
    /// <param name="diceValues">The current values of the 5 dice.</param>
    /// <param name="scorecard">The current state of the scorecard.</param>
    /// <returns>The category to fill.</returns>
    ScoreCategory ChooseCategoryToFill(IReadOnlyList<int> diceValues, Scorecard scorecard);
}
