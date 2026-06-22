using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Shiny;
using TuneGames.Models;
using TuneGames;
using TuneGames.Services;

namespace TuneGames;

[ShellMap<CategorySelectPage>]
public partial class CategorySelectViewModel(
    INavigator navigator, 
    IDialogs dialogs,
    GameSettings settings, 
    IMusicService music,
    ILogger<CategorySelectViewModel> logger
) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty]
    ObservableCollection<CategoryGroup> groups = [];

    [ObservableProperty]
    bool isLoading;

    [ObservableProperty]
    CategoryItem? selectedCategory;

    public void OnAppearing() => _ = this.LoadCategories();
    public void OnDisappearing() { }

    async Task LoadCategories()
    {
        this.IsLoading = true;
        try
        {
            var minCount = settings.TotalChoices;
            var groups = new List<CategoryGroup>();

            var playlistResults = await music.GetPlaylistsAsync();
            var playlists = playlistResults
                .Where(p => p.SongCount >= minCount)
                .Select(p => new CategoryItem(p.Name, p.SongCount, PlaylistId: p.Id))
                .ToList();
            if (playlists.Count > 0)
                groups.Add(new CategoryGroup("Playlists", playlists));

            var genreResults = await music.GetGenresAsync();
            var genres = genreResults
                .Where(g => g.Count >= minCount)
                .Select(g => new CategoryItem(g.Value, g.Count, Genre: g.Value))
                .ToList();
            if (genres.Count > 0)
                groups.Add(new CategoryGroup("Genres", genres));

            var decadeResults = await music.GetDecadesAsync();
            var decades = decadeResults
                .Where(d => d.Count >= minCount)
                .Select(d => new CategoryItem($"{d.Value}s", d.Count, Decade: d.Value))
                .ToList();
            if (decades.Count > 0)
                groups.Add(new CategoryGroup("Decades", decades));

            var yearResults = await music.GetYearsAsync();
            var years = yearResults
                .Where(y => y.Count >= minCount)
                .Select(y => new CategoryItem(y.Value.ToString(), y.Count, Year: y.Value))
                .ToList();
            if (years.Count > 0)
                groups.Add(new CategoryGroup("Years", years));

            this.Groups = new ObservableCollection<CategoryGroup>(groups);
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
    async Task CategorySelected()
    {
        var item = this.SelectedCategory;
        if (item == null)
            return;
        
        try
        {
            await navigator.NavigateTo<GamePlayViewModel>(vm =>
            {
                vm.CategoryName = item.DisplayName;
                vm.Genre = item.Genre;
                vm.Decade = item.Decade;
                vm.Year = item.Year;
                vm.PlaylistId = item.PlaylistId;
            });

            // reset so the same category can be picked again when navigating back
            this.SelectedCategory = null;
        }
        catch (Exception ex)
        {
            dialogs.Alert("Error Category Selected", ex.ToString());
            logger.LogError(ex, "Error Category Selected");
        }
    }
}

public class CategoryGroup(string name, List<CategoryItem> items) : ObservableCollection<CategoryItem>(items)
{
    public string Name { get; } = name;
}
