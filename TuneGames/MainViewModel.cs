using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using Shiny.Music;
using TuneGames.Services;
using PermissionStatus = Shiny.Music.PermissionStatus;

namespace TuneGames;

[ShellMap<MainPage>(registerRoute: false)]
public partial class MainViewModel(
    INavigator navigator,
    IDialogs dialogs,
    IMediaLibrary mediaLibrary,
    IGameStore store
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty]
    string lastScore = "";

    public async void OnAppearing()
    {
        var permission = await mediaLibrary.RequestPermissionAsync();
        if (permission != PermissionStatus.Granted)
        {
            await dialogs.Alert("Permission Denied", "Hard to play this game without the permissions.  Go to settings yourself and fix this");
            return;
        }
        
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
