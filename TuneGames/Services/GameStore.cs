using Shiny.SqliteDocumentDb;
using TuneGames.Models;

namespace TuneGames.Services;

public interface IGameStore
{
    Task<IReadOnlyList<GameResult>> GetResultsAsync();
    Task SaveResultAsync(GameResult result);
    Task ClearResultsAsync();
}

public class GameStore(IDocumentStore store) : IGameStore
{
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
