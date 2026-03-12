using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using TuneGames.Services;

namespace TuneGames.Pages.Main;

[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(INavigator navigator, IGameStore store) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty]
    string lastScore = "";

    public async void OnAppearing()
    {
        var results = await store.GetResultsAsync();
        if (results.Count > 0)
        {
            var last = results[0];
            this.LastScore = $"Last: {last.CorrectCount}/{last.TotalSongs} ({last.CategoryName})";
        }
    }

    public void OnDisappearing() { }

    [RelayCommand]
    async Task StartGame()
        => await navigator.NavigateToCategorySelect();

    [RelayCommand]
    async Task OpenSettings()
        => await navigator.NavigateToSettings();

    [RelayCommand]
    async Task ViewHistory()
        => await navigator.NavigateToHistory();
}
