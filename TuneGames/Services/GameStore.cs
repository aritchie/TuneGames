using Shiny.SqliteDocumentDb;
using TuneGames.Models;

namespace TuneGames.Services;

public interface IGameStore
{
    Task<GameSettings> GetSettingsAsync();
    Task SaveSettingsAsync(GameSettings settings);

    Task<IReadOnlyList<GameResult>> GetResultsAsync();
    Task SaveResultAsync(GameResult result);
    Task ClearResultsAsync();
}

public class GameStore(IDocumentStore store) : IGameStore
{
    public async Task<GameSettings> GetSettingsAsync()
    {
        var settings = await store.Get<GameSettings>("default");
        if (settings == null)
        {
            settings = new GameSettings();
            await store.Upsert(settings);
        }
        return settings;
    }

    public async Task SaveSettingsAsync(GameSettings settings)
    {
        settings.Id = "default";
        await store.Upsert(settings);
    }

    public async Task<IReadOnlyList<GameResult>> GetResultsAsync()
        => await store.Query<GameResult>()
            .OrderByDescending(r => r.DatePlayed)
            .ToList();

    public async Task SaveResultAsync(GameResult result)
    {
        if (string.IsNullOrEmpty(result.Id))
            result.Id = Guid.NewGuid().ToString();

        await store.Upsert(result);
    }

    public async Task ClearResultsAsync()
        => await store.Clear<GameResult>();
}
