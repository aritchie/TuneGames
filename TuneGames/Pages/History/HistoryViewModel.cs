using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using TuneGames.Models;
using TuneGames.Services;

namespace TuneGames.Pages.History;

[ShellMap<HistoryPage>]
public partial class HistoryViewModel(IGameStore store, INavigator navigator) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty]
    List<GameResult> results = [];

    [ObservableProperty]
    bool isEmpty;

    public async void OnAppearing()
    {
        this.Results = (await store.GetResultsAsync()).ToList();
        this.IsEmpty = this.Results.Count == 0;
    }

    public void OnDisappearing() { }

    [RelayCommand]
    async Task GoBack() => await navigator.GoBack();
}
