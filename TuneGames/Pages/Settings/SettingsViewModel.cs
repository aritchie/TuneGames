using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using TuneGames.Models;
using TuneGames.Services;

namespace TuneGames.Pages.Settings;

[ShellMap<SettingsPage>]
public partial class SettingsViewModel(IGameStore store, IDialogs dialogs, INavigator navigator) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty]
    int songsPerRound;

    [ObservableProperty]
    int clipDuration;

    [ObservableProperty]
    int totalChoices;

    [ObservableProperty]
    int answerTimeLimit;

    [ObservableProperty]
    int pauseBetweenClips;

    [ObservableProperty]
    List<Category> categories = [];

    public async void OnAppearing()
    {
        var settings = await store.GetSettingsAsync();
        this.SongsPerRound = settings.SongsPerRound;
        this.ClipDuration = settings.ClipDurationSeconds;
        this.TotalChoices = settings.TotalChoices;
        this.AnswerTimeLimit = settings.AnswerTimeLimitSeconds;
        this.PauseBetweenClips = settings.PauseBetweenClipsSeconds;

        this.Categories = (await store.GetCategoriesAsync()).ToList();
    }

    public void OnDisappearing() { }

    [RelayCommand]
    async Task Save()
    {
        var settings = new GameSettings
        {
            SongsPerRound = this.SongsPerRound,
            ClipDurationSeconds = this.ClipDuration,
            TotalChoices = this.TotalChoices,
            AnswerTimeLimitSeconds = this.AnswerTimeLimit,
            PauseBetweenClipsSeconds = this.PauseBetweenClips
        };
        await store.SaveSettingsAsync(settings);
        await dialogs.Alert("Saved", "Settings updated successfully.");
    }

    [RelayCommand]
    async Task AddCategory()
    {
        var name = await dialogs.Prompt("New Category", "Enter category name:", placeholder: "e.g. 90s Pop");
        if (string.IsNullOrWhiteSpace(name)) return;

        await store.SaveCategoryAsync(new Category { Name = name.Trim() });
        this.Categories = (await store.GetCategoriesAsync()).ToList();
    }

    [RelayCommand]
    async Task DeleteCategory(Category category)
    {
        var confirm = await dialogs.Confirm("Delete", $"Delete '{category.Name}'?");
        if (!confirm) return;

        await store.DeleteCategoryAsync(category.Id);
        this.Categories = (await store.GetCategoriesAsync()).ToList();
    }

    [RelayCommand]
    async Task ClearHistory()
    {
        var confirm = await dialogs.Confirm("Clear History", "Delete all game history?");
        if (!confirm) return;

        await store.ClearResultsAsync();
        await dialogs.Alert("Done", "Game history cleared.");
    }
}
