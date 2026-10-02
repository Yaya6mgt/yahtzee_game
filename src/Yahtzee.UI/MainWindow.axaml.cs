using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Yahtzee.Core.Database;
using Yahtzee.Core.Game;
using Yahtzee.Core.Models;
using Yahtzee.Core.Persistence;
using Yahtzee.UI.Views;

namespace Yahtzee.UI;

public partial class MainWindow : Window
{
    private readonly IGameDatabaseRepository _database;
    private readonly IGameSaveRepository _saveRepository;

    private NewGameView? _newGameView;
    private GameView? _gameView;
    private DatabaseView? _databaseView;

    private Action? _pendingExitAction;

    public MainWindow()
    {
        InitializeComponent();

        _database = new SqliteGameDatabase("yahtzee_games.db");
        _saveRepository = new JsonSaveRepository();

        _ = InitDatabaseAsync();

        ShowNewGameScreen();
    }

    private async Task InitDatabaseAsync()
    {
        try
        {
            await _database.InitializeDatabaseAsync();
            StatusText.Text = "Database initialized successfully.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Database init warning: {ex.Message}";
        }
    }

    private void ShowNewGameScreen()
    {
        if (CheckUnsavedChanges(() => ShowNewGameScreen())) return;

        _newGameView = new NewGameView();
        _newGameView.StartGameRequested += (s, players) => StartNewGameSession(players);

        MainContentHost.Content = _newGameView;
        HighlightNav(NavNewGameBtn);
    }

    private void StartNewGameSession(List<Player> players)
    {
        var session = new GameSession(players, _database);

        _gameView = new GameView();
        _gameView.InitializeSession(session);
        _gameView.SaveGameRequested += (s, e) => _ = SaveGameAsync();
        _gameView.PlayAgainRequested += (s, e) => ShowNewGameScreen();
        _gameView.ViewHighScoresRequested += (s, e) => ShowDatabaseScreen();

        MainContentHost.Content = _gameView;
        HighlightNav(NavNewGameBtn);
        StatusText.Text = $"Game started with {players.Count} player(s).";
    }

    private void ShowDatabaseScreen()
    {
        if (CheckUnsavedChanges(() => ShowDatabaseScreen())) return;

        _databaseView = new DatabaseView();
        _databaseView.InitializeDatabase(_database);

        MainContentHost.Content = _databaseView;
        HighlightNav(NavHighScoresBtn);
    }

    private async Task SaveGameAsync()
    {
        if (_gameView?.Session == null)
        {
            StatusText.Text = "No active game to save.";
            return;
        }

        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Yahtzee Game",
                    DefaultExtension = "json",
                    SuggestedFileName = $"yahtzee_save_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("Yahtzee Save File (*.json)") { Patterns = new[] { "*.json", "*.yahtzeesave" } }
                    }
                });

                if (file != null)
                {
                    string path = file.Path.LocalPath;
                    var saveData = _gameView.Session.ToSaveData();
                    await _saveRepository.SaveGameAsync(saveData, path);

                    _gameView.Session.HasUnsavedChanges = false;
                    StatusText.Text = $"Game saved to {Path.GetFileName(path)}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error saving game: {ex.Message}";
        }
    }

    private async Task LoadGameAsync()
    {
        if (CheckUnsavedChanges(() => _ = LoadGameAsync())) return;

        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Load Yahtzee Game",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Yahtzee Save File (*.json)") { Patterns = new[] { "*.json", "*.yahtzeesave" } }
                    }
                });

                if (files.Count > 0)
                {
                    string path = files[0].Path.LocalPath;
                    var saveData = await _saveRepository.LoadGameAsync(path);
                    var restoredSession = GameSession.RestoreFromSaveData(saveData, _database);

                    _gameView = new GameView();
                    _gameView.InitializeSession(restoredSession);
                    _gameView.SaveGameRequested += (s, e) => _ = SaveGameAsync();
                    _gameView.PlayAgainRequested += (s, e) => ShowNewGameScreen();
                    _gameView.ViewHighScoresRequested += (s, e) => ShowDatabaseScreen();

                    MainContentHost.Content = _gameView;
                    HighlightNav(NavLoadGameBtn);
                    StatusText.Text = $"Loaded game saved at {saveData.SavedAt.ToLocalTime()}";
                }
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error loading game: {ex.Message}";
        }
    }

    private bool CheckUnsavedChanges(Action continuation)
    {
        if (_gameView?.Session != null && _gameView.Session.HasUnsavedChanges && !_gameView.Session.IsGameOver)
        {
            _pendingExitAction = continuation;
            SavePromptOverlay.IsVisible = true;
            return true;
        }
        return false;
    }

    private void HighlightNav(Button btn)
    {
        NavNewGameBtn.Background = Brush.Parse("#0F172A");
        NavSaveGameBtn.Background = Brush.Parse("#0F172A");
        NavLoadGameBtn.Background = Brush.Parse("#0F172A");
        NavHighScoresBtn.Background = Brush.Parse("#0F172A");

        btn.Background = Brush.Parse("#312E81");
    }

    private void OnNavNewGameClicked(object? sender, RoutedEventArgs e) => ShowNewGameScreen();
    private void OnNavSaveGameClicked(object? sender, RoutedEventArgs e) => _ = SaveGameAsync();
    private void OnNavLoadGameClicked(object? sender, RoutedEventArgs e) => _ = LoadGameAsync();
    private void OnNavHighScoresClicked(object? sender, RoutedEventArgs e) => ShowDatabaseScreen();
    private void OnNavExitClicked(object? sender, RoutedEventArgs e)
    {
        if (CheckUnsavedChanges(() => Close())) return;
        Close();
    }

    private async void OnPromptSaveAndExitClicked(object? sender, RoutedEventArgs e)
    {
        SavePromptOverlay.IsVisible = false;
        await SaveGameAsync();
        _pendingExitAction?.Invoke();
        _pendingExitAction = null;
    }

    private void OnPromptDontSaveClicked(object? sender, RoutedEventArgs e)
    {
        SavePromptOverlay.IsVisible = false;
        if (_gameView?.Session != null)
        {
            _gameView.Session.HasUnsavedChanges = false;
        }
        _pendingExitAction?.Invoke();
        _pendingExitAction = null;
    }

    private void OnPromptCancelClicked(object? sender, RoutedEventArgs e)
    {
        SavePromptOverlay.IsVisible = false;
        _pendingExitAction = null;
    }
}