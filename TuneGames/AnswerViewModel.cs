using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using TuneGames.Models;
using TuneGames.Services;

namespace TuneGames;

[ShellMap<AnswerPage>]
public partial class AnswerViewModel(INavigator navigator, IDialogs dialogs, IGameEngine gameEngine, IGameStore store) : ObservableObject, IPageLifecycleAware, INavigationConfirmation
{
    IDispatcherTimer? timer;
    int totalTime;
    GameRound? round;

    [ShellProperty]
    public GameRound? Round
    {
        get => this.round;
        set
        {
            this.round = value;
            if (this.round != null)
                this.LoadChoices();
        }
    }

    [ObservableProperty]
    List<AnswerChoice> choices = [];

    [ObservableProperty]
    int timeRemaining;

    [ObservableProperty]
    double timerProgress = 1.0;

    [ObservableProperty]
    bool canSubmit;

    [ObservableProperty]
    string timerText = "";

    void LoadChoices()
    {
        if (this.round == null) return;

        this.totalTime = this.round.Settings.AnswerTimeLimitSeconds;
        this.TimeRemaining = this.totalTime;
        this.TimerText = $"{this.TimeRemaining}s";

        this.Choices = this.round.Picks.AllChoices
            .Select(c => new AnswerChoice(c))
            .ToList();

        this.StartTimer();
    }

    void StartTimer()
    {
        this.timer = Application.Current!.Dispatcher.CreateTimer();
        this.timer.Interval = TimeSpan.FromSeconds(1);
        this.timer.Tick += (s, e) =>
        {
            this.TimeRemaining--;
            this.TimerProgress = (double)this.TimeRemaining / this.totalTime;
            this.TimerText = $"{this.TimeRemaining}s";

            if (this.TimeRemaining <= 0)
            {
                this.timer.Stop();
                MainThread.BeginInvokeOnMainThread(async () => await this.Submit());
            }
        };
        this.timer.Start();
    }

    public async Task<bool> CanNavigate()
    {
        this.timer?.Stop();
        var exit = await dialogs.Confirm("Exit Game", "Are you sure you want to quit?");
        if (exit)
        {
            await navigator.PopToRoot();
            return false;
        }
        this.timer?.Start();
        return false;
    }

    public void OnAppearing() { }
    public void OnDisappearing() => this.timer?.Stop();

    [RelayCommand]
    void ToggleChoice(AnswerChoice choice)
    {
        choice.IsSelected = !choice.IsSelected;
        var selectedCount = this.Choices.Count(c => c.IsSelected);
        this.CanSubmit = selectedCount > 0;
    }

    [RelayCommand]
    async Task Submit()
    {
        this.timer?.Stop();

        if (this.round == null) return;

        var selectedIds = this.Choices
            .Where(c => c.IsSelected)
            .Select(c => c.Song.Id)
            .ToList();

        var result = gameEngine.SubmitAnswers(this.round, selectedIds);
        await store.SaveResultAsync(result);

        await navigator.NavigateToResults(result, this.round);
    }
}

public partial class AnswerChoice(SongChoice song) : ObservableObject
{
    public SongChoice Song { get; } = song;

    [ObservableProperty]
    bool isSelected;
}
