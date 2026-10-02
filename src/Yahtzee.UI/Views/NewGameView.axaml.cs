using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Yahtzee.Core.Models;

namespace Yahtzee.UI.Views;

public enum SelectedGameMode
{
    Solo,
    VsAi,
    Hotseat
}

public partial class NewGameView : UserControl
{
    private SelectedGameMode _currentMode = SelectedGameMode.Solo;
    private readonly List<PlayerConfigItem> _playerConfigs = new();

    public event EventHandler<List<Player>>? StartGameRequested;

    public NewGameView()
    {
        InitializeComponent();
        SetGameMode(SelectedGameMode.Solo);
    }

    private void OnModeSelected(object? sender, RoutedEventArgs e)
    {
        if (sender == SoloModeBtn) SetGameMode(SelectedGameMode.Solo);
        else if (sender == VsAiBtn) SetGameMode(SelectedGameMode.VsAi);
        else if (sender == HotseatBtn) SetGameMode(SelectedGameMode.Hotseat);
    }

    private void SetGameMode(SelectedGameMode mode)
    {
        _currentMode = mode;

        // Reset mode buttons styling
        HighlightButton(SoloModeBtn, mode == SelectedGameMode.Solo);
        HighlightButton(VsAiBtn, mode == SelectedGameMode.VsAi);
        HighlightButton(HotseatBtn, mode == SelectedGameMode.Hotseat);

        AddPlayerBtn.IsVisible = mode == SelectedGameMode.Hotseat;

        _playerConfigs.Clear();
        PlayerRowsContainer.Children.Clear();

        switch (mode)
        {
            case SelectedGameMode.Solo:
                AddPlayerRow("Player 1", isAi: false);
                break;

            case SelectedGameMode.VsAi:
                AddPlayerRow("Player 1", isAi: false);
                AddPlayerRow("Smart AI", isAi: true, defaultStrategy: "Smart");
                break;

            case SelectedGameMode.Hotseat:
                AddPlayerRow("Player 1", isAi: false);
                AddPlayerRow("Player 2", isAi: false);
                break;
        }
    }

    private void HighlightButton(Button btn, bool isSelected)
    {
        btn.Background = isSelected ? Brush.Parse("#312E81") : Brush.Parse("#1E293B");
        btn.BorderBrush = isSelected ? Brush.Parse("#6366F1") : Brush.Parse("#475569");
    }

    private void AddPlayerRow(string defaultName, bool isAi, string defaultStrategy = "Basic")
    {
        if (_playerConfigs.Count >= 4) return;

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto, Auto, Auto"),
            Margin = new Avalonia.Thickness(0, 4)
        };

        var iconText = new TextBlock
        {
            Text = isAi ? "🤖" : "👤",
            FontSize = 18,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 10, 0)
        };
        Grid.SetColumn(iconText, 0);

        var nameInput = new TextBox
        {
            Text = defaultName,
            PlaceholderText = "Enter Player Name",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(nameInput, 1);

        var aiCheck = new CheckBox
        {
            Content = "Computer AI",
            IsChecked = isAi,
            Margin = new Avalonia.Thickness(12, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = _currentMode == SelectedGameMode.Hotseat
        };
        Grid.SetColumn(aiCheck, 2);

        var strategyCombo = new ComboBox
        {
            ItemsSource = new[] { "Basic", "Smart" },
            SelectedItem = defaultStrategy,
            Margin = new Avalonia.Thickness(8, 0, 0, 0),
            IsVisible = isAi,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(strategyCombo, 3);

        aiCheck.IsCheckedChanged += (s, e) =>
        {
            strategyCombo.IsVisible = aiCheck.IsChecked == true;
            iconText.Text = aiCheck.IsChecked == true ? "🤖" : "👤";
        };

        var removeBtn = new Button
        {
            Content = "❌",
            Background = Brushes.Transparent,
            Foreground = Brush.Parse("#EF4444"),
            Margin = new Avalonia.Thickness(6, 0, 0, 0),
            IsVisible = _currentMode == SelectedGameMode.Hotseat && _playerConfigs.Count >= 2,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(removeBtn, 4);

        var configItem = new PlayerConfigItem(nameInput, aiCheck, strategyCombo, grid);
        _playerConfigs.Add(configItem);

        removeBtn.Click += (s, e) =>
        {
            if (_playerConfigs.Count > 2)
            {
                _playerConfigs.Remove(configItem);
                PlayerRowsContainer.Children.Remove(grid);
            }
        };

        grid.Children.Add(iconText);
        grid.Children.Add(nameInput);
        grid.Children.Add(aiCheck);
        grid.Children.Add(strategyCombo);
        grid.Children.Add(removeBtn);

        PlayerRowsContainer.Children.Add(grid);
    }

    private void OnAddPlayerClicked(object? sender, RoutedEventArgs e)
    {
        AddPlayerRow($"Player {_playerConfigs.Count + 1}", isAi: false);
    }

    private void OnStartGameClicked(object? sender, RoutedEventArgs e)
    {
        var players = new List<Player>();
        for (int i = 0; i < _playerConfigs.Count; i++)
        {
            var item = _playerConfigs[i];
            string name = string.IsNullOrWhiteSpace(item.NameInput.Text)
                ? $"Player {i + 1}"
                : item.NameInput.Text.Trim();

            bool isAi = item.AiCheck.IsChecked == true;
            string strat = item.StrategyCombo.SelectedItem?.ToString() ?? "Basic";

            players.Add(isAi ? new AiPlayer(name, strat) : new HumanPlayer(name));
        }

        StartGameRequested?.Invoke(this, players);
    }

    private record PlayerConfigItem(TextBox NameInput, CheckBox AiCheck, ComboBox StrategyCombo, Grid Container);
}
