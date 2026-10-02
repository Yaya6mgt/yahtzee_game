using Yahtzee.Core.Scoring;

namespace Yahtzee.Core.Models;

public abstract class Player
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; }
    public Scorecard Scorecard { get; }
    public abstract bool IsAi { get; }

    protected Player(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be empty.", nameof(name));

        Name = name.Trim();
        Scorecard = new Scorecard();
    }
}

public class HumanPlayer : Player
{
    public override bool IsAi => false;

    public HumanPlayer(string name) : base(name) { }
}

public class AiPlayer : Player
{
    public override bool IsAi => true;
    public string StrategyName { get; }

    public AiPlayer(string name, string strategyName = "Basic") : base(name)
    {
        StrategyName = strategyName;
    }
}
