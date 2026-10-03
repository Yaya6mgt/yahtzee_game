using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
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

        RoundText.Text = $"ROUND {_session.CurrentRound} / {GameSession.TotalRounds}";
        TurnText.Text = $"{_session.CurrentPlayer.Name}'s Turn";
        TurnIconText.Text = "";

        int rolls = _session.DiceCup.RollsRemaining;
        RollsText.Text = $"{rolls} Roll{(rolls == 1 ? "" : "s")} Remaining";
        RollsBadge.Background = rolls switch
        {
            3 => Brush.Parse("#065F46"),
            2 => Brush.Parse("#92400E"),
            1 => Brush.Parse("#9A3412"),
            _ => Brush.Parse("#991B1B")
        };

        bool canRoll = _session.DiceCup.CanRoll && !_session.CurrentPlayer.IsAi;
        RollBtn.IsEnabled = canRoll;
        RollBtn.Content = rolls > 0 ? $"🎲 ROLL DICE ({rolls})" : "NO ROLLS LEFT";
        RollBtn.Background = canRoll ? GetThemeBrush("PrimaryBrush", "#10B981") : GetThemeBrush("BorderSubtleBrush", "#2E3138");

        var dice = _session.DiceCup.Dice;
        SetDieState(Die0, dice[0]);
        SetDieState(Die1, dice[1]);
        SetDieState(Die2, dice[2]);
        SetDieState(Die3, dice[3]);
        SetDieState(Die4, dice[4]);

        RenderPlayerTabs();

        RenderScorecardRows();

        var sc = _session.CurrentPlayer.Scorecard;
        UpperSubtotalText.Text = $"{sc.UpperSectionSubtotal} / {Scorecard.UpperSectionBonusThreshold} pts";
        UpperBonusText.Text = sc.UpperSectionBonus > 0 ? $"+{sc.UpperSectionBonus} PTS (UNLOCKED)" : $"{sc.UpperSectionSubtotal} / 63 pts";
        LowerTotalText.Text = $"{sc.LowerSectionTotal} pts";
        GrandTotalText.Text = $"{sc.TotalScore} PTS";

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

        if (toggled) RefreshUi();
    }

    private void OnRollClicked(object? sender, RoutedEventArgs e)
    {
        if (_session == null || _session.CurrentPlayer.IsAi) return;

        if (_session.RollDice()) RefreshUi();
    }

    private async void CheckAiTurn()
    {
        if (_session == null || _session.IsGameOver) return;

        if (_session.CurrentPlayer.IsAi)
        {
            RollBtn.IsEnabled = false;
            AiStatusBanner.IsVisible = true;

            await Task.Delay(600);
            await _session.ExecuteAiTurnStepAsync(() =>
            {
                RefreshUi();
            });

            RefreshUi();
            CheckAiTurn();
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
                Background = isCurrent ? Brush.Parse("#0B382A") : Brush.Parse("#12151F"),
                BorderBrush = isCurrent ? Brush.Parse("#D4AF37") : Brush.Parse("#252B36"),
                BorderThickness = new Avalonia.Thickness(isCurrent ? 1.5 : 1),
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(12, 6)
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };

            if (isCurrent)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = "●",
                    FontSize = 10,
                    Foreground = Brush.Parse("#10B981"),
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            if (player.IsAi)
            {
                var aiBorder = new Border
                {
                    Background = Brush.Parse("#2D1D06"),
                    CornerRadius = new Avalonia.CornerRadius(4),
                    Padding = new Avalonia.Thickness(5, 1),
                    VerticalAlignment = VerticalAlignment.Center
                };
                aiBorder.Child = new TextBlock
                {
                    Text = "BOT",
                    FontSize = 9,
                    FontWeight = FontWeight.Black,
                    Foreground = Brush.Parse("#FBBF24")
                };
                stack.Children.Add(aiBorder);
            }

            stack.Children.Add(new TextBlock
            {
                Text = player.Name,
                FontWeight = isCurrent ? FontWeight.Black : FontWeight.SemiBold,
                Foreground = isCurrent ? Brushes.White : Brush.Parse("#A1A5B0"),
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center
            });

            var scorePill = new Border
            {
                Background = isCurrent ? Brush.Parse("#24200A") : Brush.Parse("#0B0E14"),
                CornerRadius = new Avalonia.CornerRadius(4),
                Padding = new Avalonia.Thickness(6, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            scorePill.Child = new TextBlock
            {
                Text = $"{player.Scorecard.TotalScore} pts",
                FontSize = 11,
                FontWeight = FontWeight.Bold,
                Foreground = isCurrent ? Brush.Parse("#FFD700") : Brush.Parse("#F59E0B")
            };
            stack.Children.Add(scorePill);

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

        int upperSubtotal = scorecard.UpperSectionSubtotal;
        int neededForBonus = Math.Max(0, Scorecard.UpperSectionBonusThreshold - upperSubtotal);
        bool hasBonus = scorecard.UpperSectionBonus > 0;

        var upperHeader = new Border
        {
            Background = Brush.Parse("#0D201A"),
            BorderBrush = Brush.Parse("#10B981"),
            BorderThickness = new Avalonia.Thickness(1),
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(12, 8),
            Margin = new Avalonia.Thickness(0, 2, 0, 4)
        };
        var upperGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("*, Auto") };

        var upperTitleStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        upperTitleStack.Children.Add(new TextBlock { Text = "🎯", FontSize = 14, VerticalAlignment = VerticalAlignment.Center });
        upperTitleStack.Children.Add(new TextBlock { Text = "UPPER SECTION • NUMBERS (1 to 6)", FontWeight = FontWeight.Black, Foreground = Brush.Parse("#34D399"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(upperTitleStack, 0);

        var upperBonusInfo = new TextBlock
        {
            Text = hasBonus ? "+35 PTS BONUS UNLOCKED!" : $"+35 Pts bonus target: {neededForBonus} pts needed",
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            Foreground = hasBonus ? Brush.Parse("#FFD700") : Brush.Parse("#A1A5B0"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(upperBonusInfo, 1);

        upperGrid.Children.Add(upperTitleStack);
        upperGrid.Children.Add(upperBonusInfo);
        upperHeader.Child = upperGrid;
        ScorecardRowsContainer.Children.Add(upperHeader);

        foreach (ScoreCategory category in Enum.GetValues<ScoreCategory>().Where(c => (int)c <= 6))
        {
            RenderSingleRow(category, scorecard, previews, hasRolled, isAiTurn);
        }

        var lowerHeader = new Border
        {
            Background = Brush.Parse("#231018"),
            BorderBrush = Brush.Parse("#BE123C"),
            BorderThickness = new Avalonia.Thickness(1),
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(12, 8),
            Margin = new Avalonia.Thickness(0, 10, 0, 4)
        };
        var lowerGrid = new Grid { ColumnDefinitions = new ColumnDefinitions("*, Auto") };

        var lowerTitleStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        lowerTitleStack.Children.Add(new TextBlock { Text = "🃏", FontSize = 14, VerticalAlignment = VerticalAlignment.Center });
        lowerTitleStack.Children.Add(new TextBlock { Text = "LOWER SECTION • CASINO COMBINATIONS", FontWeight = FontWeight.Black, Foreground = Brush.Parse("#FB7185"), FontSize = 12, VerticalAlignment = VerticalAlignment.Center });
        Grid.SetColumn(lowerTitleStack, 0);

        var lowerInfo = new TextBlock
        {
            Text = "Poker & VIP Figures",
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brush.Parse("#A1A5B0"),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(lowerInfo, 1);

        lowerGrid.Children.Add(lowerTitleStack);
        lowerGrid.Children.Add(lowerInfo);
        lowerHeader.Child = lowerGrid;
        ScorecardRowsContainer.Children.Add(lowerHeader);

        foreach (ScoreCategory category in Enum.GetValues<ScoreCategory>().Where(c => (int)c >= 7))
        {
            RenderSingleRow(category, scorecard, previews, hasRolled, isAiTurn);
        }
    }

    private void RenderSingleRow(
        ScoreCategory category,
        Scorecard scorecard,
        IReadOnlyDictionary<ScoreCategory, int> previews,
        bool hasRolled,
        bool isAiTurn)
    {
        bool isFilled = scorecard.IsCategoryFilled(category);
        int? filledScore = scorecard.GetScore(category);
        int previewScore = previews[category];
        var details = GetCasinoCategoryDetails(category);

        var border = new Border
        {
            CornerRadius = new Avalonia.CornerRadius(8),
            Padding = new Avalonia.Thickness(10, 7),
            Margin = new Avalonia.Thickness(0, 2)
        };

        var rowGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto, Auto")
        };

        if (isFilled)
        {
            if (filledScore > 0)
            {
                border.Background = Brush.Parse("#0E261D");
                border.BorderBrush = Brush.Parse("#66D4AF37");
                border.BorderThickness = new Avalonia.Thickness(1.5);
            }
            else
            {
                border.Background = Brush.Parse("#1E1116");
                border.BorderBrush = Brush.Parse("#66BE123C");
                border.BorderThickness = new Avalonia.Thickness(1);
            }
        }
        else if (hasRolled)
        {
            border.Background = Brush.Parse("#10B981");
            border.BorderBrush = previewScore > 0 ? Brush.Parse("#D4AF37") : Brush.Parse("#982E2E");
            border.BorderThickness = new Avalonia.Thickness(1);
            border.Cursor = isAiTurn ? new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow) : new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);

            if (!isAiTurn)
            {
                ScoreCategory capturedCat = category;
                border.PointerPressed += (s, e) => OnScoreCategoryClicked(capturedCat);
            }
        }
        else
        {
            border.Background = Brush.Parse("#10B981");
            border.BorderBrush = Brush.Parse("#1B5643");
            border.BorderThickness = new Avalonia.Thickness(1);
        }

        var leftStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, VerticalAlignment = VerticalAlignment.Center };

        var iconBorder = new Border
        {
            Background = Brush.Parse(details.BadgeBgHex),
            CornerRadius = new Avalonia.CornerRadius(6),
            Width = 28,
            Height = 28,
            VerticalAlignment = VerticalAlignment.Center
        };
        var iconText = new TextBlock
        {
            Text = details.Icon,
            FontSize = 14,
            Foreground = Brushes.White,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        iconBorder.Child = iconText;
        leftStack.Children.Add(iconBorder);

        var titleStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Spacing = 1 };
        var titleText = new TextBlock
        {
            Text = details.Title,
            FontWeight = isFilled ? FontWeight.Bold : FontWeight.SemiBold,
            FontSize = 13,
            Foreground = isFilled ? (filledScore > 0 ? Brush.Parse("#F8FAFC") : Brush.Parse("#A1A5B0")) : Brushes.White
        };
        titleStack.Children.Add(titleText);

        leftStack.Children.Add(titleStack);
        Grid.SetColumn(leftStack, 0);

        var descText = new TextBlock
        {
            Text = details.Description,
            FontSize = 10.5,
            Foreground = isFilled ? Brush.Parse("#F1FFE2") : Brush.Parse("#033018"),
            Margin = new Avalonia.Thickness(10, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(descText, 1);

        if (isFilled)
        {
            var scoreChip = new Border
            {
                CornerRadius = new Avalonia.CornerRadius(6),
                Padding = new Avalonia.Thickness(12, 4),
                Margin = new Avalonia.Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            if (filledScore > 0)
            {
                scoreChip.Background = Brush.Parse("#26200A");
                scoreChip.BorderBrush = Brush.Parse("#D4AF37");
                scoreChip.BorderThickness = new Avalonia.Thickness(1);
                var t = new TextBlock
                {
                    Text = $"✓ {filledScore} PTS",
                    FontSize = 12.5,
                    FontWeight = FontWeight.Black,
                    Foreground = Brush.Parse("#FFD700")
                };
                scoreChip.Child = t;
            }
            else
            {
                scoreChip.Background = Brush.Parse("#2B1218");
                scoreChip.BorderBrush = Brush.Parse("#80BE123C");
                scoreChip.BorderThickness = new Avalonia.Thickness(1);
                var t = new TextBlock
                {
                    Text = "✕ 0 PT",
                    FontSize = 11,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brush.Parse("#FB7185")
                };
                scoreChip.Child = t;
            }

            Grid.SetColumn(scoreChip, 2);
            Grid.SetColumnSpan(scoreChip, 2);
            rowGrid.Children.Add(scoreChip);
        }
        else if (hasRolled)
        {
            var previewChip = new Border
            {
                CornerRadius = new Avalonia.CornerRadius(5),
                Padding = new Avalonia.Thickness(8, 3),
                Margin = new Avalonia.Thickness(8, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            if (previewScore > 0)
            {
                previewChip.Background = Brush.Parse("#382606");
                previewChip.BorderBrush = Brush.Parse("#F59E0B");
                previewChip.BorderThickness = new Avalonia.Thickness(1);
                var pText = new TextBlock
                {
                    Text = $"+{previewScore} PTS",
                    FontSize = 12,
                    FontWeight = FontWeight.Black,
                    Foreground = Brush.Parse("#FBBF24")
                };
                previewChip.Child = pText;
            }
            else
            {
                previewChip.Background = Brush.Parse("#2B1218");
                previewChip.BorderBrush = Brush.Parse("#9F1239");
                previewChip.BorderThickness = new Avalonia.Thickness(1);
                var pText = new TextBlock
                {
                    Text = "0 PT",
                    FontSize = 10,
                    FontWeight = FontWeight.Bold,
                    Foreground = Brush.Parse("#FDA4AF")
                };
                previewChip.Child = pText;
            }
            Grid.SetColumn(previewChip, 2);
            rowGrid.Children.Add(previewChip);

            var selectBtn = new Button
            {
                Content = "SCORE",
                Background = previewScore > 0 ? Brush.Parse("#10B981") : Brush.Parse("#9F1239"),
                Foreground = Brushes.White,
                Padding = new Avalonia.Thickness(12, 5),
                CornerRadius = new Avalonia.CornerRadius(6),
                FontWeight = FontWeight.Black,
                FontSize = 11,
                IsEnabled = !isAiTurn,
                VerticalAlignment = VerticalAlignment.Center
            };
            ScoreCategory capturedCat = category;
            selectBtn.Click += (s, e) => OnScoreCategoryClicked(capturedCat);
            Grid.SetColumn(selectBtn, 3);
            rowGrid.Children.Add(selectBtn);
        }
        else
        {
            var placeholderBorder = new Border
            {
                Background = Brush.Parse("#08533A"),
                CornerRadius = new Avalonia.CornerRadius(4),
                Padding = new Avalonia.Thickness(10, 3),
                Margin = new Avalonia.Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var placeholderText = new TextBlock
            {
                Text = "--",
                FontSize = 12,
                FontWeight = FontWeight.Bold,
                Foreground = Brush.Parse("#B0D2BF")
            };
            placeholderBorder.Child = placeholderText;
            Grid.SetColumn(placeholderBorder, 2);
            Grid.SetColumnSpan(placeholderBorder, 2);
            rowGrid.Children.Add(placeholderBorder);
        }

        rowGrid.Children.Add(leftStack);
        rowGrid.Children.Add(descText);

        border.Child = rowGrid;
        ScorecardRowsContainer.Children.Add(border);
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

        WinnerText.Text = $"🏆 {winner.Name} wins with {winner.Scorecard.TotalScore} Points! 🏆";
        FinalStandingsContainer.Children.Clear();

        for (int i = 0; i < winners.Count; i++)
        {
            var p = winners[i];
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto"),
                Margin = new Avalonia.Thickness(0, 4)
            };

            string rankLabel = i switch
            {
                0 => "🥇 1st",
                1 => "🥈 2nd",
                2 => "🥉 3rd",
                _ => $"{i + 1}th"
            };

            var rankText = new TextBlock
            {
                Text = rankLabel,
                FontWeight = FontWeight.Black,
                Foreground = i == 0 ? Brush.Parse("#FFD700") : Brushes.White,
                Margin = new Avalonia.Thickness(0, 0, 12, 0),
                FontSize = 14
            };
            Grid.SetColumn(rankText, 0);

            var nameText = new TextBlock
            {
                Text = p.Name + (p.IsAi ? " (BOT)" : ""),
                FontWeight = FontWeight.Bold,
                Foreground = Brushes.White,
                FontSize = 14
            };
            Grid.SetColumn(nameText, 1);

            var scoreText = new TextBlock
            {
                Text = $"{p.Scorecard.TotalScore} PTS",
                FontWeight = FontWeight.Black,
                Foreground = Brush.Parse("#10B981"),
                FontSize = 14
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

    private static (string Icon, string BadgeBgHex, string Title, string Description) GetCasinoCategoryDetails(ScoreCategory c) => c switch
    {
        ScoreCategory.Aces => ("⚀", "#044D3A", "ACES", "Sum of all 1"),
        ScoreCategory.Twos => ("⚁", "#044D3A", "TWOS", "Sum of all 2"),
        ScoreCategory.Threes => ("⚂", "#044D3A", "THREES", "Sum of all 3"),
        ScoreCategory.Fours => ("⚃", "#044D3A", "FOURS", "Sum of all 4"),
        ScoreCategory.Fives => ("⚄", "#044D3A", "FIVES", "Sum of all 5"),
        ScoreCategory.Sixes => ("⚅", "#044D3A", "SIXES", "Sum of all 6"),

        ScoreCategory.ThreeOfAKind => ("🎰", "#8B152B", "3 OF A KIND", "At least 3 matching dice → Sum of all 5 dice"),
        ScoreCategory.FourOfAKind => ("🔥", "#BE123C", "4 OF A KIND", "At least 4 matching dice → Sum of all 5 dice"),
        ScoreCategory.FullHouse => ("🏠", "#D97706", "FULL HOUSE", "3 of one kind + 2 of another → 25 pts"),
        ScoreCategory.SmallStraight => ("⚡", "#059669", "SMALL STRAIGHT", "Sequence of 4 dice → 30 pts"),
        ScoreCategory.LargeStraight => ("👑", "#7C3AED", "LARGE STRAIGHT", "Sequence of 5 dice → 40 pts"),
        ScoreCategory.Yahtzee => ("💎", "#D4AF37", "YAHTZEE VIP!", "All 5 dice matching → 50 pts"),
        ScoreCategory.Chance => ("🎲", "#0D9488", "CHANCE", "Any combination → Sum of all 5 dice"),
        _ => ("🎲", "#686D7A", c.ToString(), "")
    };

    private static IBrush GetThemeBrush(string key, string fallbackHex)
    {
        if (Application.Current?.TryFindResource(key, out var res) == true && res is IBrush brush)
        {
            return brush;
        }
        return Brush.Parse(fallbackHex);
    }
}
