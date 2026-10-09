namespace Yahtzee.Core.Scoring;

/// <summary>
/// Defines the 13 official scoring categories of a Yahtzee scorecard.
/// </summary>
public enum ScoreCategory
{
    /// <summary>Sum of dice with value 1.</summary>
    Aces = 1,

    /// <summary>Sum of dice with value 2.</summary>
    Twos = 2,

    /// <summary>Sum of dice with value 3.</summary>
    Threes = 3,

    /// <summary>Sum of dice with value 4.</summary>
    Fours = 4,

    /// <summary>Sum of dice with value 5.</summary>
    Fives = 5,

    /// <summary>Sum of dice with value 6.</summary>
    Sixes = 6,

    /// <summary>At least 3 matching dice; scores sum of all 5 dice.</summary>
    ThreeOfAKind = 7,

    /// <summary>At least 4 matching dice; scores sum of all 5 dice.</summary>
    FourOfAKind = 8,

    /// <summary>3 of one value and 2 of another value; scores 25 points.</summary>
    FullHouse = 9,

    /// <summary>Sequence of 4 sequential dice; scores 30 points.</summary>
    SmallStraight = 10,

    /// <summary>Sequence of 5 sequential dice; scores 40 points.</summary>
    LargeStraight = 11,

    /// <summary>All 5 dice matching; scores 50 points.</summary>
    Yahtzee = 12,

    /// <summary>Any combination of dice; scores sum of all 5 dice.</summary>
    Chance = 13
}
