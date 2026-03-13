using CarPlay;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shiny.Music;
using TuneGames.Models;
using TuneGames.Services;

namespace TuneGames;

public class CarPlayGameManager
{
    readonly CPInterfaceController interfaceController;
    readonly Action onGameExit;
    IServiceScope? scope;
    GameRound? round;
    HashSet<string> selectedSongIds = [];
    CancellationTokenSource? playbackCts;
    bool isCleanedUp;

    public CarPlayGameManager(CPInterfaceController interfaceController, Action onGameExit)
    {
        this.interfaceController = interfaceController;
        this.onGameExit = onGameExit;
    }

    public void StartGame(string categoryName)
    {
        this.ShowLoading(categoryName);
        _ = this.RunGameAsync(categoryName);
    }

    void ShowLoading(string categoryName)
    {
        var template = new CPInformationTemplate(
            $"🎵 {categoryName}",
            CPInformationTemplateLayout.TwoColumn,
            [new CPInformationItem("Loading...", "AI is picking songs")],
            []
        );
        template.BackButton = new CPBarButton("Cancel", _ => this.onGameExit());
        this.interfaceController.PushTemplate(template, true, null);
    }

    async Task RunGameAsync(string categoryName)
    {
        try
        {
            var services = IPlatformApplication.Current!.Services;
            this.scope = services.CreateScope();
            var engine = this.scope.ServiceProvider.GetRequiredService<IGameEngine>();
            var settings = await this.scope.ServiceProvider.GetRequiredService<IGameStore>().GetSettingsAsync();

            this.round = await engine.StartRoundAsync(
                categoryName,
                new MusicFilter { Genre = categoryName },
                settings
            );

            if (this.isCleanedUp) return;

            this.ShowPlayingTemplate(categoryName, 0, this.round.Settings.SongsPerRound);

            this.playbackCts = new CancellationTokenSource();
            await engine.PlayClipsAsync(this.round, (current, total) =>
            {
                if (!this.isCleanedUp)
                    this.ShowPlayingTemplate(categoryName, current, total);
            }, this.playbackCts.Token);

            if (this.isCleanedUp) return;

            this.ShowAnswerTemplate();
        }
        catch (OperationCanceledException)
        {
            // Playback cancelled during cleanup
        }
        catch (Exception ex)
        {
            if (this.isCleanedUp) return;

            var logger = IPlatformApplication.Current!.Services.GetRequiredService<ILogger<CarPlayGameManager>>();
            logger.LogError(ex, "CarPlay game failed");
            this.ShowError(ex.Message);
        }
    }

    void ShowPlayingTemplate(string categoryName, int current, int total)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (this.isCleanedUp) return;

            var items = new List<CPInformationItem>
            {
                new($"🎧 Clip {current}/{total}", categoryName),
                new("Listen carefully!", "Name the songs you hear")
            };

            var template = new CPInformationTemplate(
                "Now Playing",
                CPInformationTemplateLayout.TwoColumn,
                items.ToArray(),
                []
            );
            template.BackButton = new CPBarButton("Cancel", _ => this.onGameExit());

            this.interfaceController.PopToRootTemplate(false, null);
            this.interfaceController.PushTemplate(template, true, null);
        });
    }

    void ShowAnswerTemplate()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (this.isCleanedUp || this.round == null) return;

            this.selectedSongIds.Clear();
            var template = this.BuildAnswerList();
            this.interfaceController.PopToRootTemplate(false, null);
            this.interfaceController.PushTemplate(template, true, null);
        });
    }

    CPListTemplate BuildAnswerList()
    {
        var items = this.round!.Picks.AllChoices.Select(song =>
        {
            var isSelected = this.selectedSongIds.Contains(song.Id);
            var prefix = isSelected ? "✅ " : "";
            var item = new CPListItem($"{prefix}{song.Title}", song.Artist)
            {
                Handler = (listItem, completion) =>
                {
                    this.ToggleSong(song.Id);
                    completion();
                }
            };
            return (ICPListTemplateItem)item;
        }).ToArray();

        var section = new CPListSection(items, $"Pick {this.round.Settings.SongsPerRound} songs you heard", null);

        var submitButton = new CPBarButton("Submit", _ => this.SubmitAnswers());

        var template = new CPListTemplate("🧠 Name That Tune!", [section]);
        template.BackButton = new CPBarButton("Cancel", _ => this.onGameExit());
        template.TrailingNavigationBarButtons = [submitButton];
        return template;
    }

    void ToggleSong(string songId)
    {
        if (!this.selectedSongIds.Remove(songId))
        {
            if (this.round != null && this.selectedSongIds.Count >= this.round.Settings.SongsPerRound)
            {
                var alert = new CPAlertTemplate(
                    [$"You can only pick {this.round.Settings.SongsPerRound} songs"],
                    [new CPAlertAction("OK", CPAlertActionStyle.Default, _ =>
                        this.interfaceController.DismissTemplate(true, null))]
                );
                this.interfaceController.PresentTemplate(alert, true, null);
                return;
            }
            this.selectedSongIds.Add(songId);
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (this.isCleanedUp || this.round == null) return;

            var template = this.BuildAnswerList();
            this.interfaceController.PopTemplate(false, null);
            this.interfaceController.PushTemplate(template, false, null);
        });
    }

    void SubmitAnswers()
    {
        if (this.round == null) return;

        var services = IPlatformApplication.Current!.Services;
        var engine = services.GetRequiredService<IGameEngine>();
        var result = engine.SubmitAnswers(this.round, this.selectedSongIds.ToList());

        _ = this.SaveAndShowResults(result);
    }

    async Task SaveAndShowResults(GameResult result)
    {
        try
        {
            var services = IPlatformApplication.Current!.Services;
            var store = services.GetRequiredService<IGameStore>();
            await store.SaveResultAsync(result);
        }
        catch (Exception ex)
        {
            var logger = IPlatformApplication.Current!.Services.GetRequiredService<ILogger<CarPlayGameManager>>();
            logger.LogError(ex, "Failed to save CarPlay game result");
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (this.isCleanedUp) return;

            var emoji = result.CorrectCount == result.TotalSongs ? "🏆" :
                        result.CorrectCount > 0 ? "🎉" : "😅";

            var items = new List<CPInformationItem>
            {
                new($"{emoji} {result.CorrectCount}/{result.TotalSongs}", "Score"),
                new(result.CategoryName, "Category")
            };

            foreach (var song in result.SongsPlayed)
                items.Add(new("🎵", song));

            var template = new CPInformationTemplate(
                "Results",
                CPInformationTemplateLayout.TwoColumn,
                items.ToArray(),
                [new CPTextButton("Play Again", CPTextButtonStyle.Confirm, _ => this.onGameExit())]
            );
            template.BackButton = new CPBarButton("Done", _ => this.onGameExit());

            this.interfaceController.PopToRootTemplate(false, null);
            this.interfaceController.PushTemplate(template, true, null);
        });
    }

    void ShowError(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (this.isCleanedUp) return;

            var template = new CPInformationTemplate(
                "Oops!",
                CPInformationTemplateLayout.TwoColumn,
                [new CPInformationItem("⚠️", message)],
                [new CPTextButton("Back", CPTextButtonStyle.Cancel, _ => this.onGameExit())]
            );

            this.interfaceController.PopToRootTemplate(false, null);
            this.interfaceController.PushTemplate(template, true, null);
        });
    }

    public void Cleanup()
    {
        this.isCleanedUp = true;
        this.playbackCts?.Cancel();
        this.playbackCts?.Dispose();
        this.playbackCts = null;
        this.scope?.Dispose();
        this.scope = null;
        this.round = null;
    }
}
