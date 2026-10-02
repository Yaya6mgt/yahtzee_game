namespace Yahtzee.Core.Scoring;

public enum ScoreCategory
{
    // Upper Section
    Aces = 1,
    Twos = 2,
    Threes = 3,
    Fours = 4,
    Fives = 5,
    Sixes = 6,

    // Lower Section
    ThreeOfAKind = 7,
    FourOfAKind = 8,
    FullHouse = 9,
    SmallStraight = 10,
    LargeStraight = 11,
    Yahtzee = 12,
    Chance = 13
}
