using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Yahtzee.Core.Game;
using Yahtzee.Core.Models;
using Yahtzee.Core.Scoring;
using Yahtzee.UI.Controls;

namespace Yahtzee.UI.Views;

public partial class GameView : UserControl
{
    private GameSession? _session;

    public event EventHandler? SaveGameRequested;
    public event EventHandler? PlayAgainRequested;
    public event EventHandler? ViewHighScoresRequested;

    public GameSession? Session => _session;

    public GameView()
    {
        InitializeComponent();
        SetupDiceEvents();
    }

    public void InitializeSession(GameSession session)
    {
        _session = session;
        _session.TurnChanged += (s, e) => RefreshUi();
        _session.GameOver += (s, e) => ShowGameOverModal();

        RefreshUi();
        CheckAiTurn();
    }

    private void SetupDiceEvents()
    {
        Die0.ToggleHoldRequested += (s, e) => OnDieHoldToggled(0);
        Die1.ToggleHoldRequested += (s, e) => OnDieHoldToggled(1);
        Die2.ToggleHoldRequested += (s, e) => OnDieHoldToggled(2);
        Die3.ToggleHoldRequested += (s, e) => OnDieHoldToggled(3);
        Die4.ToggleHoldRequested += (s, e) => OnDieHoldToggled(4);
    }

    private void RefreshUi()
    {
        if (_session == null) return;

        // Top Status
        RoundText.Text = $"ROUND {_session.CurrentRound} / {GameSession.TotalRounds}";
        TurnText.Text = $"{_session.CurrentPlayer.Name}'s Turn";
        TurnIconText.Text = _session.CurrentPlayer.IsAi ? "🤖" : "🎯";

        int rolls = _session.DiceCup.RollsRemaining;
        RollsText.Text = $"🎲 {rolls} Roll{(rolls == 1 ? "" : "s")} Left";
        RollsBadge.Background = rolls switch
        {
            3 => Brush.Parse("#065F46"),
            2 => Brush.Parse("#92400E"),
            1 => Brush.Parse("#9A3412"),
            _ => Brush.Parse("#991B1B")
        };

        // Roll Button State
        bool canRoll = _session.DiceCup.CanRoll && !_session.CurrentPlayer.IsAi;
        RollBtn.IsEnabled = canRoll;
        RollBtn.Content = rolls > 0 ? $"🎲 ROLL DICE ({rolls} left)" : "❌ NO ROLLS LEFT";
        RollBtn.Background = canRoll ? Brush.Parse("#6366F1") : Brush.Parse("#475569");

        // Dice Displays
        var dice = _session.DiceCup.Dice;
        SetDieState(Die0, dice[0]);
        SetDieState(Die1, dice[1]);
        SetDieState(Die2, dice[2]);
        SetDieState(Die3, dice[3]);
        SetDieState(Die4, dice[4]);

        // Player Tabs
        RenderPlayerTabs();

        // Scorecard Rows
        RenderScorecardRows();

        // Totals
        var sc = _session.CurrentPlayer.Scorecard;
        UpperSubtotalText.Text = $"{sc.UpperSectionSubtotal} / {Scorecard.UpperSectionBonusThreshold}";
        UpperBonusText.Text = sc.UpperSectionBonus > 0 ? $"+{sc.UpperSectionBonus}" : "0";
        LowerTotalText.Text = sc.LowerSectionTotal.ToString();
        GrandTotalText.Text = sc.TotalScore.ToString();

        // AI Status
        AiStatusBanner.IsVisible = _session.CurrentPlayer.IsAi;
        if (_session.CurrentPlayer.IsAi)
        {
            AiStatusText.Text = $"{_session.CurrentPlayer.Name} is taking turn...";
        }
    }

    private void SetDieState(DiceControl ctrl, Die die)
    {
        ctrl.Value = die.Value;
        ctrl.IsHeld = die.IsHeld;
    }

    private void OnDieHoldToggled(int index)
    {
        if (_session == null || _session.CurrentPlayer.IsAi) return;

        bool toggled = _session.ToggleHold(index);
        if (toggled)
        {
            RefreshUi();
        }
    }

    private void OnRollClicked(object? sender, RoutedEventArgs e)
    {
        if (_session == null || _session.CurrentPlayer.IsAi) return;

        if (_session.RollDice())
        {
            RefreshUi();
        }
    }

    private async void CheckAiTurn()
    {
        if (_session == null || _session.IsGameOver) return;

        if (_session.CurrentPlayer.IsAi)
        {
            RollBtn.IsEnabled = false;
            AiStatusBanner.IsVisible = true;

            await Task.Delay(600); // UI visual delay
            await _session.ExecuteAiTurnStepAsync(() =>
            {
                RefreshUi();
            });

            RefreshUi();
            CheckAiTurn(); // Loop if consecutive AI players
        }
    }

    private void RenderPlayerTabs()
    {
        if (_session == null) return;
        PlayerTabsContainer.Children.Clear();

        for (int i = 0; i < _session.Players.Count; i++)
        {
            var player = _session.Players[i];
            bool isCurrent = i == _session.CurrentPlayerIndex;

            var border = new Border
            {
                Background = isCurrent ? Brush.Parse("#312E81") : Brush.Parse("#0F172A"),
                BorderBrush = isCurrent ? Brush.Parse("#6366F1") : Brush.Parse("#334155"),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(6),
                Padding = new Avalonia.Thickness(12, 6)
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            stack.Children.Add(new TextBlock
            {
                Text = player.IsAi ? "🤖" : "👤",
                FontSize = 14
            });
            stack.Children.Add(new TextBlock
            {
                Text = player.Name,
                FontWeight = isCurrent ? FontWeight.Bold : FontWeight.Normal,
                Foreground = isCurrent ? Brushes.White : Brush.Parse("#94A3B8")
            });
            stack.Children.Add(new TextBlock
            {
                Text = $"({player.Scorecard.TotalScore} pts)",
                FontSize = 11,
                Foreground = Brush.Parse("#F59E0B"),
                VerticalAlignment = VerticalAlignment.Center
            });

            border.Child = stack;
            PlayerTabsContainer.Children.Add(border);
        }
    }

    private void RenderScorecardRows()
    {
        if (_session == null) return;
        ScorecardRowsContainer.Children.Clear();

        var scorecard = _session.CurrentPlayer.Scorecard;
        var previews = ScoreCalculator.PreviewAllScores(_session.DiceCup.GetValues());
        bool hasRolled = _session.DiceCup.HasRolledThisRound;
        bool isAiTurn = _session.CurrentPlayer.IsAi;

        foreach (ScoreCategory category in Enum.GetValues<ScoreCategory>())
        {
            bool isFilled = scorecard.IsCategoryFilled(category);
            int? filledScore = scorecard.GetScore(category);
            int previewScore = previews[category];

            var rowGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto, Auto"),
                Margin = new Avalonia.Thickness(0, 2)
            };

            var border = new Border
            {
                Background = isFilled ? Brush.Parse("#0F172A") : Brush.Parse("#1E293B"),
                BorderBrush = isFilled ? Brush.Parse("#334155") : Brush.Parse("#475569"),
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(6),
                Padding = new Avalonia.Thickness(12, 8)
            };

            // Category Icon & Name
            var nameStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
            nameStack.Children.Add(new TextBlock
            {
                Text = GetCategoryIcon(category),
                FontSize = 14
            });
            nameStack.Children.Add(new TextBlock
            {
                Text = GetCategoryDisplayName(category),
                FontWeight = FontWeight.SemiBold,
                Foreground = isFilled ? Brush.Parse("#94A3B8") : Brushes.White
            });
            Grid.SetColumn(nameStack, 0);

            // Description / Rule
            var descText = new TextBlock
            {
                Text = GetCategoryDescription(category),
                FontSize = 11,
                Foreground = Brush.Parse("#64748B"),
                Margin = new Avalonia.Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(descText, 1);

            // Score Display / Preview Badge
            if (isFilled)
            {
                var filledScoreText = new TextBlock
                {
                    Text = $"{filledScore} pts",
                    FontWeight = FontWeight.Bold,
                    Foreground = Brush.Parse("#10B981"),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Avalonia.Thickness(12, 0, 12, 0)
                };
                Grid.SetColumn(filledScoreText, 2);
                rowGrid.Children.Add(filledScoreText);
            }
            else
            {
                var previewBorder = new Border
                {
                    Background = hasRolled ? Brush.Parse("#312E81") : Brush.Parse("#0F172A"),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Padding = new Avalonia.Thickness(8, 2),
                    Margin = new Avalonia.Thickness(12, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var previewText = new TextBlock
                {
                    Text = hasRolled ? $"+{previewScore} pts" : "-",
                    FontSize = 12,
                    FontWeight = FontWeight.Bold,
                    Foreground = hasRolled ? (previewScore > 0 ? Brush.Parse("#F59E0B") : Brush.Parse("#94A3B8")) : Brush.Parse("#64748B")
                };
                previewBorder.Child = previewText;
                Grid.SetColumn(previewBorder, 2);
                rowGrid.Children.Add(previewBorder);

                // Select Score Button
                var selectBtn = new Button
                {
                    Content = "Score",
                    Background = Brush.Parse("#10B981"),
                    Foreground = Brushes.White,
                    Padding = new Avalonia.Thickness(10, 4),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    IsEnabled = hasRolled && !isAiTurn,
                    VerticalAlignment = VerticalAlignment.Center
                };
                ScoreCategory capturedCategory = category;
                selectBtn.Click += (s, e) => OnScoreCategoryClicked(capturedCategory);
                Grid.SetColumn(selectBtn, 3);
                rowGrid.Children.Add(selectBtn);
            }

            rowGrid.Children.Add(nameStack);
            rowGrid.Children.Add(descText);

            border.Child = rowGrid;
            ScorecardRowsContainer.Children.Add(border);
        }
    }

    private void OnScoreCategoryClicked(ScoreCategory category)
    {
        if (_session == null || _session.CurrentPlayer.IsAi) return;

        if (_session.SelectCategory(category))
        {
            RefreshUi();
            CheckAiTurn();
        }
    }

    private void ShowGameOverModal()
    {
        if (_session == null) return;

        GameOverOverlay.IsVisible = true;
        var winners = _session.GetRankedPlayers();
        var winner = winners.First();

        WinnerText.Text = $"🎉 {winner.Name} Wins with {winner.Scorecard.TotalScore} Points!";
        FinalStandingsContainer.Children.Clear();

        for (int i = 0; i < winners.Count; i++)
        {
            var p = winners[i];
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto"),
                Margin = new Avalonia.Thickness(0, 4)
            };

            var rankText = new TextBlock
            {
                Text = i == 0 ? "🥇 1st" : (i == 1 ? "🥈 2nd" : (i == 2 ? "🥉 3rd" : $"{i + 1}th")),
                FontWeight = FontWeight.Bold,
                Foreground = i == 0 ? Brush.Parse("#F59E0B") : Brushes.White,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            Grid.SetColumn(rankText, 0);

            var nameText = new TextBlock
            {
                Text = p.Name,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brushes.White
            };
            Grid.SetColumn(nameText, 1);

            var scoreText = new TextBlock
            {
                Text = $"{p.Scorecard.TotalScore} pts",
                FontWeight = FontWeight.Bold,
                Foreground = Brush.Parse("#10B981")
            };
            Grid.SetColumn(scoreText, 2);

            grid.Children.Add(rankText);
            grid.Children.Add(nameText);
            grid.Children.Add(scoreText);

            FinalStandingsContainer.Children.Add(grid);
        }
    }

    private void OnSaveGameClicked(object? sender, RoutedEventArgs e)
    {
        SaveGameRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnGameOverViewHighScoresClicked(object? sender, RoutedEventArgs e)
    {
        ViewHighScoresRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnGameOverPlayAgainClicked(object? sender, RoutedEventArgs e)
    {
        GameOverOverlay.IsVisible = false;
        PlayAgainRequested?.Invoke(this, EventArgs.Empty);
    }

    private static string GetCategoryIcon(ScoreCategory c) => c switch
    {
        ScoreCategory.Aces => "1️⃣",
        ScoreCategory.Twos => "2️⃣",
        ScoreCategory.Threes => "3️⃣",
        ScoreCategory.Fours => "4️⃣",
        ScoreCategory.Fives => "5️⃣",
        ScoreCategory.Sixes => "6️⃣",
        ScoreCategory.ThreeOfAKind => "☘️",
        ScoreCategory.FourOfAKind => "🍀",
        ScoreCategory.FullHouse => "🏠",
        ScoreCategory.SmallStraight => "🪜",
        ScoreCategory.LargeStraight => "🚀",
        ScoreCategory.Yahtzee => "⭐",
        ScoreCategory.Chance => "🎰",
        _ => "🎲"
    };

    private static string GetCategoryDisplayName(ScoreCategory c) => c switch
    {
        ScoreCategory.Aces => "Aces",
        ScoreCategory.Twos => "Twos",
        ScoreCategory.Threes => "Threes",
        ScoreCategory.Fours => "Fours",
        ScoreCategory.Fives => "Fives",
        ScoreCategory.Sixes => "Sixes",
        ScoreCategory.ThreeOfAKind => "3 of a Kind",
        ScoreCategory.FourOfAKind => "4 of a Kind",
        ScoreCategory.FullHouse => "Full House",
        ScoreCategory.SmallStraight => "Small Straight",
        ScoreCategory.LargeStraight => "Large Straight",
        ScoreCategory.Yahtzee => "YAHTZEE",
        ScoreCategory.Chance => "Chance",
        _ => c.ToString()
    };

    private static string GetCategoryDescription(ScoreCategory c) => c switch
    {
        ScoreCategory.Aces => "Sum of 1s",
        ScoreCategory.Twos => "Sum of 2s",
        ScoreCategory.Threes => "Sum of 3s",
        ScoreCategory.Fours => "Sum of 4s",
        ScoreCategory.Fives => "Sum of 5s",
        ScoreCategory.Sixes => "Sum of 6s",
        ScoreCategory.ThreeOfAKind => "At least 3 same -> Sum all",
        ScoreCategory.FourOfAKind => "At least 4 same -> Sum all",
        ScoreCategory.FullHouse => "3 of one & 2 of another -> 25",
        ScoreCategory.SmallStraight => "4 sequence dice -> 30",
        ScoreCategory.LargeStraight => "5 sequence dice -> 40",
        ScoreCategory.Yahtzee => "All 5 dice same -> 50",
        ScoreCategory.Chance => "Sum of all dice",
        _ => ""
    };
}
