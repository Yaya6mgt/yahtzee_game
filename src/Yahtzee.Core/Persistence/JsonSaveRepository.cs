using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yahtzee.Core.Persistence;

/// <summary>
/// Repository interface for saving and loading game sessions.
/// </summary>
public interface IGameSaveRepository
{
    /// <summary>
    /// Saves game state data to a specified file path.
    /// </summary>
    /// <param name="saveData">The game state DTO to save.</param>
    /// <param name="filePath">Target destination file path.</param>
    Task SaveGameAsync(GameSaveData saveData, string filePath);

    /// <summary>
    /// Loads game state data from a specified file path.
    /// </summary>
    /// <param name="filePath">Source file path to load.</param>
    /// <returns>Deserialized game save data DTO.</returns>
    Task<GameSaveData> LoadGameAsync(string filePath);

    /// <summary>
    /// Enumerates saved game files present in a specified directory.
    /// </summary>
    /// <param name="directoryPath">Directory path to scan.</param>
    /// <returns>Collection of save file paths.</returns>
    IEnumerable<string> ListSaveFiles(string directoryPath);
}

/// <summary>
/// JSON implementation of <see cref="IGameSaveRepository"/> using System.Text.Json.
/// </summary>
public class JsonSaveRepository : IGameSaveRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Saves the game to a file.
    /// </summary>
    /// <param name="saveData">The game data to save.</param>
    /// <param name="filePath">The path to the file to save to.</param>
    public async Task SaveGameAsync(GameSaveData saveData, string filePath)
    {
        ArgumentNullException.ThrowIfNull(saveData);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string dir = Path.GetDirectoryName(filePath)!;
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, saveData, Options);
    }

    /// <summary>
    /// Loads the game from a file.
    /// </summary>
    /// <param name="filePath">The path to the file to load from.</param>
    /// <returns>The game data.</returns>
    public async Task<GameSaveData> LoadGameAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Save file not found at: {filePath}", filePath);

        using var stream = File.OpenRead(filePath);
        var data = await JsonSerializer.DeserializeAsync<GameSaveData>(stream, Options);
        return data ?? throw new InvalidDataException("Failed to deserialize save game file.");
    }

    /// <summary>
    /// Lists all save files in a directory.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to search for save files.</param>
    /// <returns>An enumerable of save file paths.</returns>
    public IEnumerable<string> ListSaveFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            return Enumerable.Empty<string>();

        return Directory.GetFiles(directoryPath, "*.yahtzeesave")
            .Concat(Directory.GetFiles(directoryPath, "*.json"))
            .OrderByDescending(File.GetLastWriteTimeUtc);
    }
}
