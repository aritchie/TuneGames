using Shiny.SqliteDocumentDb;
using TuneGames.Models;

namespace TuneGames.Services;

public interface IGameStore
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync();
    Task SaveCategoryAsync(Category category);
    Task DeleteCategoryAsync(string id);

    Task<GameSettings> GetSettingsAsync();
    Task SaveSettingsAsync(GameSettings settings);

    Task<IReadOnlyList<GameResult>> GetResultsAsync();
    Task SaveResultAsync(GameResult result);
    Task ClearResultsAsync();
}

public class GameStore(IDocumentStore store) : IGameStore
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        var list = await store.Query<Category>().ToList();
        if (list.Count == 0)
        {
            var defaults = new[]
            {
                new Category { Id = Guid.NewGuid().ToString(), Name = "80s Rock", Description = "Classic rock from the 1980s" },
                new Category { Id = Guid.NewGuid().ToString(), Name = "Pop Hits", Description = "Popular mainstream hits" },
                new Category { Id = Guid.NewGuid().ToString(), Name = "Hip Hop", Description = "Rap and hip hop tracks" },
                new Category { Id = Guid.NewGuid().ToString(), Name = "Country", Description = "Country music" },
                new Category { Id = Guid.NewGuid().ToString(), Name = "Jazz", Description = "Jazz standards and classics" }
            };
            foreach (var c in defaults)
                await store.Upsert(c);

            return defaults;
        }
        return list;
    }

    public async Task SaveCategoryAsync(Category category)
    {
        if (string.IsNullOrEmpty(category.Id))
            category.Id = Guid.NewGuid().ToString();

        await store.Upsert(category);
    }

    public async Task DeleteCategoryAsync(string id)
        => await store.Remove<Category>(id);

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
