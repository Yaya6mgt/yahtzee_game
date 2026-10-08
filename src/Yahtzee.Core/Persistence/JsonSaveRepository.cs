using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yahtzee.Core.Persistence;

public interface IGameSaveRepository
{
    Task SaveGameAsync(GameSaveData saveData, string filePath);
    Task<GameSaveData> LoadGameAsync(string filePath);
    IEnumerable<string> ListSaveFiles(string directoryPath);
}

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
