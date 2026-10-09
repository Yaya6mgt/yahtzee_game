using Yahtzee.Core.Scoring;

namespace Yahtzee.Core.Models;

/// <summary>
/// Abstract base class representing a participant in a Yahtzee game session.
/// </summary>
public abstract class Player
{
    /// <summary>
    /// The unique identifier of the player.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// The name of the player.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The scorecard of the player.
    /// </summary>
    public Scorecard Scorecard { get; }

    /// <summary>
    /// Gets a value indicating whether the player is an AI.
    /// </summary>
    public abstract bool IsAi { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="name">The name of the player.</param>
    protected Player(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be empty.", nameof(name));

        Name = name.Trim();
        Scorecard = new Scorecard();
    }
}

/// <summary>
/// Represents a human player.
/// </summary>
public class HumanPlayer : Player
{
    /// <inheritdoc />
    public override bool IsAi => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="HumanPlayer"/> class.
    /// </summary>
    /// <param name="name">The name of the player.</param>
    public HumanPlayer(string name) : base(name) { }
}

/// <summary>
/// Represents an AI player.
/// </summary>
public class AiPlayer : Player
{
    /// <inheritdoc />
    public override bool IsAi => true;

    /// <summary>
    /// Gets the name of the AI decision strategy associated with this bot.
    /// </summary>
    public string StrategyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AiPlayer"/> class.
    /// </summary>
    /// <param name="name">The name of the AI player.</param>
    /// <param name="strategyName">The name of the AI strategy to use.</param>
    public AiPlayer(string name, string strategyName = "Basic") : base(name)
    {
        StrategyName = strategyName;
    }
}
