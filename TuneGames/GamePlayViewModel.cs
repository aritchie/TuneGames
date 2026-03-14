using CommunityToolkit.Mvvm.ComponentModel;
using Shiny;
using Shiny.Music;
using TuneGames.Models;
using TuneGames.Services;

namespace TuneGames;

[ShellMap<GamePlayPage>]
public partial class GamePlayViewModel(
    INavigator navigator,
    IDialogs dialogs,
    IGameEngine gameEngine,
    IGameStore store
) : ObservableObject, IPageLifecycleAware, INavigationConfirmation
{
    CancellationTokenSource? playCts;
    GameRound? round;

    [property: ShellProperty]
    [ObservableProperty]
    string categoryName = "";

    [property: ShellProperty]
    [ObservableProperty]
    string? genre;

    [property: ShellProperty]
    [ObservableProperty]
    int? decade;

    [property: ShellProperty]
    [ObservableProperty]
    int? year;

    [property: ShellProperty]
    [ObservableProperty]
    string? playlistId;

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

    public async Task<bool> CanNavigate()
        => await dialogs.Confirm("Exit Game", "Are you sure you want to quit?");

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

            this.round = await gameEngine.StartRoundAsync(
                this.CategoryName,
                new MusicFilter { Genre = this.Genre, Decade = this.Decade, Year = this.Year },
                settings,
                this.PlaylistId
            );

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
