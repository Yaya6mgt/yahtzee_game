using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Yahtzee.Core.Database;

namespace Yahtzee.UI.Views;

public partial class DatabaseView : UserControl
{
    private IGameDatabaseRepository? _database;
    private int _selectedTab = 0; // 0 = Top Overall, 1 = By Player, 2 = History

    public DatabaseView()
    {
        InitializeComponent();
    }

    public void InitializeDatabase(IGameDatabaseRepository database)
    {
        _database = database;
        _ = LoadDataAsync();
    }

    private void OnFilterTabClicked(object? sender, RoutedEventArgs e)
    {
        if (sender == TopOverallBtn) SwitchTab(0);
        else if (sender == TopByPlayerBtn) SwitchTab(1);
        else if (sender == RecentHistoryBtn) SwitchTab(2);
    }

    private void SwitchTab(int tabIndex)
    {
        _selectedTab = tabIndex;

        HighlightButton(TopOverallBtn, tabIndex == 0);
        HighlightButton(TopByPlayerBtn, tabIndex == 1);
        HighlightButton(RecentHistoryBtn, tabIndex == 2);

        PlayerSearchContainer.IsVisible = tabIndex == 1;

        _ = LoadDataAsync();
    }

    private void HighlightButton(Button btn, bool isActive)
    {
        btn.Background = isActive ? Brush.Parse("#312E81") : Brush.Parse("#0F172A");
        btn.Foreground = isActive ? Brushes.White : Brush.Parse("#94A3B8");
    }

    private void OnPlayerSearchClicked(object? sender, RoutedEventArgs e)
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        if (_database == null) return;
        LeaderboardRowsContainer.Children.Clear();
        EmptyStateText.IsVisible = false;

        if (_selectedTab == 0)
        {
            var records = await _database.GetTop5HighScoresOverallAsync();
            RenderHighScoreRows(records);
        }
        else if (_selectedTab == 1)
        {
            string name = PlayerSearchInput.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(name))
            {
                EmptyStateText.Text = "Enter a player name above and click Search.";
                EmptyStateText.IsVisible = true;
                return;
            }

            var records = await _database.GetTop5HighScoresForPlayerAsync(name);
            RenderHighScoreRows(records);
        }
        else if (_selectedTab == 2)
        {
            var history = await _database.GetRecentGamesAsync(15);
            RenderHistoryRows(history);
        }
    }

    private void RenderHighScoreRows(List<HighScoreRecord> records)
    {
        if (records.Count == 0)
        {
            EmptyStateText.Text = "No high score records found.";
            EmptyStateText.IsVisible = true;
            return;
        }

        for (int i = 0; i < records.Count; i++)
        {
            var r = records[i];
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("60, *, 120, 100, 160"),
                Margin = new Avalonia.Thickness(0, 2)
            };

            var border = new Border
            {
                Background = Brush.Parse("#0F172A"),
                CornerRadius = new Avalonia.CornerRadius(6),
                Padding = new Avalonia.Thickness(12, 8)
            };

            var rankText = new TextBlock
            {
                Text = i == 0 ? "🥇 1st" : (i == 1 ? "🥈 2nd" : (i == 2 ? "🥉 3rd" : $"{i + 1}th")),
                FontWeight = FontWeight.Bold,
                Foreground = i == 0 ? Brush.Parse("#F59E0B") : Brushes.White
            };
            Grid.SetColumn(rankText, 0);

            var nameText = new TextBlock
            {
                Text = r.PlayerName,
                FontWeight = FontWeight.SemiBold,
                Foreground = Brushes.White
            };
            Grid.SetColumn(nameText, 1);

            var scoreText = new TextBlock
            {
                Text = $"{r.Score} pts",
                FontWeight = FontWeight.Black,
                Foreground = Brush.Parse("#10B981")
            };
            Grid.SetColumn(scoreText, 2);

            var typeText = new TextBlock
            {
                Text = r.IsAi ? "🤖 AI" : "👤 Human",
                Foreground = Brush.Parse("#A5B4FC")
            };
            Grid.SetColumn(typeText, 3);

            var dateText = new TextBlock
            {
                Text = r.PlayedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                FontSize = 11,
                Foreground = Brush.Parse("#64748B")
            };
            Grid.SetColumn(dateText, 4);

            grid.Children.Add(rankText);
            grid.Children.Add(nameText);
            grid.Children.Add(scoreText);
            grid.Children.Add(typeText);
            grid.Children.Add(dateText);

            border.Child = grid;
            LeaderboardRowsContainer.Children.Add(border);
        }
    }

    private void RenderHistoryRows(List<CompletedGameRecord> history)
    {
        if (history.Count == 0)
        {
            EmptyStateText.Text = "No completed games recorded yet.";
            EmptyStateText.IsVisible = true;
            return;
        }

        for (int i = 0; i < history.Count; i++)
        {
            var h = history[i];
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("60, *, 120, 100, 160"),
                Margin = new Avalonia.Thickness(0, 2)
            };

            var border = new Border
            {
                Background = Brush.Parse("#0F172A"),
                CornerRadius = new Avalonia.CornerRadius(6),
                Padding = new Avalonia.Thickness(12, 8)
            };

            var rankText = new TextBlock { Text = $"#{i + 1}", Foreground = Brush.Parse("#94A3B8") };
            Grid.SetColumn(rankText, 0);

            var winnerText = new TextBlock { Text = $"🏆 {h.WinnerName}", FontWeight = FontWeight.Bold, Foreground = Brushes.White };
            Grid.SetColumn(winnerText, 1);

            var scoreText = new TextBlock { Text = $"{h.WinnerScore} pts", FontWeight = FontWeight.Bold, Foreground = Brush.Parse("#F59E0B") };
            Grid.SetColumn(scoreText, 2);

            var countText = new TextBlock { Text = $"{h.PlayerCount} Players", Foreground = Brush.Parse("#94A3B8") };
            Grid.SetColumn(countText, 3);

            var dateText = new TextBlock { Text = h.PlayedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), FontSize = 11, Foreground = Brush.Parse("#64748B") };
            Grid.SetColumn(dateText, 4);

            grid.Children.Add(rankText);
            grid.Children.Add(winnerText);
            grid.Children.Add(scoreText);
            grid.Children.Add(countText);
            grid.Children.Add(dateText);

            border.Child = grid;
            LeaderboardRowsContainer.Children.Add(border);
        }
    }
}
