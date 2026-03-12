using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using TuneGames.Models;

namespace TuneGames.Pages.Results;

[ShellMap<ResultsPage>]
public partial class ResultsViewModel(INavigator navigator) : ObservableObject
{
    GameResult? result;
    GameRound? round;

    [ShellProperty]
    public GameResult? Result
    {
        get => this.result;
        set
        {
            this.result = value;
            this.OnPropertyChanged();
            if (this.result != null) this.BuildDisplay();
        }
    }

    [ShellProperty]
    public GameRound? Round
    {
        get => this.round;
        set
        {
            this.round = value;
            this.OnPropertyChanged();
            if (this.round != null && this.result != null) this.BuildDisplay();
        }
    }

    [ObservableProperty]
    string scoreText = "";

    [ObservableProperty]
    string emoji = "";

    [ObservableProperty]
    List<ResultItem> resultItems = [];

    void BuildDisplay()
    {
        if (this.result == null || this.round == null) return;

        this.ScoreText = $"{this.result.CorrectCount} / {this.result.TotalSongs}";
        this.Emoji = this.result.CorrectCount == this.result.TotalSongs ? "🏆" :
                this.result.CorrectCount > 0 ? "👏" : "😅";

        var correctIds = this.round.Picks.SongsToPlay.Select(s => s.Id).ToHashSet();
        var pickedIds = this.result.SongsPicked
            .Select((_, i) =>
            {
                var selected = this.round.Picks.AllChoices
                    .Where(c => this.result.SongsPicked.Contains($"{c.Title} - {c.Artist}"))
                    .Select(c => c.Id)
                    .ToHashSet();
                return selected;
            })
            .FirstOrDefault() ?? [];

        this.ResultItems = this.round.Picks.AllChoices
            .Select(c =>
            {
                var wasPlayed = correctIds.Contains(c.Id);
                var wasSelected = this.result.SongsPicked.Contains($"{c.Title} - {c.Artist}");

                var status = (wasPlayed, wasSelected) switch
                {
                    (true, true) => ResultStatus.Correct,
                    (true, false) => ResultStatus.Missed,
                    (false, true) => ResultStatus.Wrong,
                    _ => ResultStatus.None
                };

                return new ResultItem(c.Title, c.Artist, c.Album, status);
            })
            .OrderByDescending(r => r.Status == ResultStatus.Correct)
            .ThenByDescending(r => r.Status == ResultStatus.Missed)
            .ThenByDescending(r => r.Status == ResultStatus.Wrong)
            .ToList();
    }

    [RelayCommand]
    async Task PlayAgain() => await navigator.PopToRoot();

    [RelayCommand]
    async Task GoHome() => await navigator.PopToRoot();
}

public record ResultItem(string Title, string Artist, string? Album, ResultStatus Status);

public enum ResultStatus { None, Correct, Missed, Wrong }
