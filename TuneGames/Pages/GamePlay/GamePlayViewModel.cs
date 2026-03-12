using CommunityToolkit.Mvvm.ComponentModel;
using Shiny;
using TuneGames.Models;
using TuneGames.Services;

namespace TuneGames.Pages.GamePlay;

[ShellMap<GamePlayPage>]
public partial class GamePlayViewModel(
    INavigator navigator,
    IDialogs dialogs,
    IGameEngine gameEngine,
    IGameStore store
) : ObservableObject, IPageLifecycleAware
{
    CancellationTokenSource? playCts;
    GameRound? round;

    [property: ShellProperty]
    [ObservableProperty]
    string categoryName = "";

    [ObservableProperty]
    string statusText = "Loading...";

    [ObservableProperty]
    string currentSongInfo = "";

    [ObservableProperty]
    int currentSongNumber;

    [ObservableProperty]
    int totalSongs;

    [ObservableProperty]
    double progress;

    [ObservableProperty]
    GamePhase phase = GamePhase.Loading;

    public async void OnAppearing() => await this.StartRound();

    public void OnDisappearing()
    {
        this.playCts?.Cancel();
        this.playCts?.Dispose();
    }

    async Task StartRound()
    {
        try
        {
            this.Phase = GamePhase.Loading;
            this.StatusText = "AI is picking songs...";

            var settings = await store.GetSettingsAsync();
            this.TotalSongs = settings.SongsPerRound;

            this.round = await gameEngine.StartRoundAsync(this.CategoryName, settings);

            this.Phase = GamePhase.Playing;
            this.StatusText = "Listen carefully!";

            this.playCts = new CancellationTokenSource();

            await gameEngine.PlayClipsAsync(this.round, (current, total) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    this.CurrentSongNumber = current;
                    this.Progress = (double)current / total;
                    this.CurrentSongInfo = $"Playing song {current} of {total}";
                });
            }, this.playCts.Token);

            await navigator.NavigateToAnswer(this.round);
        }
        catch (OperationCanceledException)
        {
            // User left the page
        }
        catch (Exception ex)
        {
            await dialogs.Alert("Error", ex.Message);
            await navigator.GoBack();
        }
    }
}
