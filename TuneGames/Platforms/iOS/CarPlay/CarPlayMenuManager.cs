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

            var categories = await store.GetCategoriesAsync();
            var categoryItems = categories.Select(c =>
            {
                var item = new CPListItem(c.Name, c.Description ?? "Tap to play")
                {
                    Handler = (listItem, completion) =>
                    {
                        this.onStartGame(c.Name);
                        completion();
                    }
                };
                return (ICPListTemplateItem)item;
            }).ToArray();

            var categorySection = new CPListSection(categoryItems, "Pick a Category", null);

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
