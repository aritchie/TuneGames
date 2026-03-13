using CarPlay;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TuneGames.Services;

namespace TuneGames;

public class CarPlayMenuManager
{
    readonly CPInterfaceController interfaceController;
    readonly Action<string> onStartGame;

    public CarPlayMenuManager(CPInterfaceController interfaceController, Action<string> onStartGame)
    {
        this.interfaceController = interfaceController;
        this.onStartGame = onStartGame;
    }

    public void Show()
    {
        var template = this.BuildTemplate([], []);
        this.interfaceController.SetRootTemplate(template, false, null);
        _ = this.LoadData();
    }

    CPListTemplate BuildTemplate(CPListSection[] categorySections, CPListSection[] historySections)
    {
        var allSections = categorySections
            .Concat(historySections)
            .ToArray();

        return new CPListTemplate("🎵 TUNE Games", allSections);
    }

    async Task LoadData()
    {
        try
        {
            var services = IPlatformApplication.Current!.Services;
            var store = services.GetRequiredService<IGameStore>();
            var music = services.GetRequiredService<IMusicService>();

            var settings = await store.GetSettingsAsync();
            var minCount = settings.TotalChoices;

            var genres = await music.GetGenresAsync();
            var categoryItems = genres
                .Where(g => g.Count >= minCount)
                .Select(g =>
                {
                    var item = new CPListItem(g.Value, $"{g.Count} songs")
                    {
                        Handler = (listItem, completion) =>
                        {
                            this.onStartGame(g.Value);
                            completion();
                        }
                    };
                    return (ICPListTemplateItem)item;
                }).ToArray();

            var categorySection = new CPListSection(categoryItems, "Pick a Genre", null);

            var results = await store.GetResultsAsync();
            var historySections = Array.Empty<CPListSection>();

            if (results.Count > 0)
            {
                var historyItems = results
                    .Take(10)
                    .Select(r =>
                    {
                        var score = $"{r.CorrectCount}/{r.TotalSongs}";
                        var detail = $"{r.CategoryName} — {r.DatePlayed:MMM d, h:mm tt}";
                        var item = new CPListItem(score, detail);
                        return (ICPListTemplateItem)item;
                    }).ToArray();

                historySections = [new CPListSection(historyItems, "Recent Games", null)];
            }

            var template = this.BuildTemplate([categorySection], historySections);
            this.interfaceController.SetRootTemplate(template, true, null);
        }
        catch (Exception ex)
        {
            var logger = IPlatformApplication.Current!.Services.GetRequiredService<ILogger<CarPlayMenuManager>>();
            logger.LogError(ex, "Failed to load CarPlay menu");
        }
    }
}
