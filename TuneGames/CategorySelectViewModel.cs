using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using TuneGames.Models;
using TuneGames;
using TuneGames.Services;

namespace TuneGames;

[ShellMap<CategorySelectPage>]
public partial class CategorySelectViewModel(
    INavigator navigator, 
    IDialogs dialogs,
    IGameStore store, 
    IMusicService music
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty]
    List<CategoryItem> genres = [];

    [ObservableProperty]
    List<CategoryItem> decades = [];

    [ObservableProperty]
    List<CategoryItem> years = [];

    [ObservableProperty]
    List<CategoryItem> playlists = [];

    [ObservableProperty]
    bool isLoading;

    public void OnAppearing() => _ = this.LoadCategories();
    public void OnDisappearing() { }

    async Task LoadCategories()
    {
        this.IsLoading = true;
        try
        {

            var settings = await store.GetSettingsAsync();
            var minCount = settings.TotalChoices;

            var genreResults = await music.GetGenresAsync();
            this.Genres = genreResults
                .Where(g => g.Count >= minCount)
                .Select(g => new CategoryItem(g.Value, g.Count, Genre: g.Value))
                .ToList();

            var decadeResults = await music.GetDecadesAsync();
            this.Decades = decadeResults
                .Where(d => d.Count >= minCount)
                .Select(d => new CategoryItem($"{d.Value}s", d.Count, Decade: d.Value))
                .ToList();

            var yearResults = await music.GetYearsAsync();
            this.Years = yearResults
                .Where(y => y.Count >= minCount)
                .Select(y => new CategoryItem(y.Value.ToString(), y.Count, Year: y.Value))
                .ToList();

            var playlistResults = await music.GetPlaylistsAsync();
            this.Playlists = playlistResults
                .Where(p => p.SongCount >= minCount)
                .Select(p => new CategoryItem(p.Name, p.SongCount, PlaylistId: p.Id))
                .ToList();
        }
        catch (Exception ex)
        {
            dialogs.Alert("Error Loading Categories", ex.ToString());
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    [RelayCommand]
    Task SelectCategory(CategoryItem item)
        => navigator.NavigateTo<GamePlayViewModel>(vm =>
        {
            vm.CategoryName = item.DisplayName;
            vm.Genre = item.Genre;
            vm.Decade = item.Decade;
            vm.Year = item.Year;
            vm.PlaylistId = item.PlaylistId;
        });
}
