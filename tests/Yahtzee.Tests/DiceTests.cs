using Yahtzee.Core.Models;
using Yahtzee.Core.Services;

namespace Yahtzee.Tests;

public class TestRandomProvider : IRandomProvider
{
    private readonly Queue<int> _values;

    public TestRandomProvider(IEnumerable<int> values)
    {
        _values = new Queue<int>(values);
    }

    public int NextDieValue() => _values.Count > 0 ? _values.Dequeue() : 1;
}

public class DiceTests
{
    [Fact]
    public void Die_DefaultValueIsOne_AndNotHeld()
    {
        var die = new Die();
        Assert.Equal(1, die.Value);
        Assert.False(die.IsHeld);
    }

    [Fact]
    public void Die_RollsNewValue_WhenNotHeld()
    {
        var die = new Die();
        var fakeRandom = new TestRandomProvider([5]);

        die.Roll(fakeRandom);

        Assert.Equal(5, die.Value);
    }

    [Fact]
    public void Die_DoesNotRoll_WhenHeld()
    {
        var die = new Die(initialValue: 3, isHeld: true);
        var fakeRandom = new TestRandomProvider([6]);

        die.Roll(fakeRandom);

        Assert.Equal(3, die.Value);
    }

    [Fact]
    public void DiceCup_InitialState_HasFiveDice_AndThreeRollsRemaining()
    {
        var cup = new DiceCup();
        Assert.Equal(5, cup.Dice.Count);
        Assert.Equal(3, cup.RollsRemaining);
        Assert.True(cup.CanRoll);
        Assert.False(cup.HasRolledThisRound);
    }

    [Fact]
    public void DiceCup_Roll_DecrementsRollsRemaining_AndUpdatesDiceValues()
    {
        var fakeRandom = new TestRandomProvider([2, 4, 6, 1, 3]);
        var cup = new DiceCup(fakeRandom);

        bool result = cup.Roll();

        Assert.True(result);
        Assert.Equal(2, cup.RollsRemaining);
        Assert.True(cup.HasRolledThisRound);
        Assert.Equal([2, 4, 6, 1, 3], cup.GetValues());
    }

    [Fact]
    public void DiceCup_ToggleHold_BeforeFirstRoll_ReturnsFalse()
    {
        var cup = new DiceCup();
        bool result = cup.ToggleHold(0);

        Assert.False(result);
        Assert.False(cup.Dice[0].IsHeld);
    }

    [Fact]
    public void DiceCup_ToggleHold_AfterFirstRoll_TogglesState()
    {
        var cup = new DiceCup();
        cup.Roll();

        bool result = cup.ToggleHold(0);

        Assert.True(result);
        Assert.True(cup.Dice[0].IsHeld);

        cup.ToggleHold(0);
        Assert.False(cup.Dice[0].IsHeld);
    }

    [Fact]
    public void DiceCup_Roll_OnlyRollsUnheldDice()
    {
        // First roll
        var fakeRandom = new TestRandomProvider([1, 1, 1, 1, 1, 6, 6, 6, 6, 6]);
        var cup = new DiceCup(fakeRandom);
        cup.Roll(); // Dice are all 1

        cup.SetHold(0, true);
        cup.SetHold(1, true);

        cup.Roll(); // Unheld dice (indices 2,3,4) get rolled to 6, held stay 1

        Assert.Equal([1, 1, 6, 6, 6], cup.GetValues());
    }

    [Fact]
    public void DiceCup_ResetForNewRound_ResetsRollsAndUnheldsAllDice()
    {
        var cup = new DiceCup();
        cup.Roll();
        cup.SetHold(0, true);
        cup.Roll();

        cup.ResetForNewRound();

        Assert.Equal(3, cup.RollsRemaining);
        Assert.False(cup.HasRolledThisRound);
        Assert.All(cup.Dice, d => Assert.False(d.IsHeld));
    }
}
