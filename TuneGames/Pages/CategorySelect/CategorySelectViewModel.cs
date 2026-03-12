using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shiny;
using TuneGames.Models;
using TuneGames.Pages.GamePlay;
using TuneGames.Services;

namespace TuneGames.Pages.CategorySelect;

[ShellMap<CategorySelectPage>]
public partial class CategorySelectViewModel(INavigator navigator, IDialogs dialogs, IGameStore store) : ObservableObject, IPageLifecycleAware
{
    [ObservableProperty]
    List<Category> categories = [];

    [ObservableProperty]
    bool isLoading;

    public void OnAppearing() => _ = this.LoadCategories();
    public void OnDisappearing() { }

    async Task LoadCategories()
    {
        this.IsLoading = true;
        try
        {
            this.Categories = (await store.GetCategoriesAsync()).ToList();
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    [RelayCommand]
    Task SelectCategory(Category category)
        => navigator.NavigateTo<GamePlayViewModel>(vm => vm.CategoryName = category.Name);

    [RelayCommand]
    async Task AddCategory()
    {
        var name = await dialogs.Prompt("New Category", "Enter category name:", placeholder: "e.g. 90s Pop");
        if (!String.IsNullOrWhiteSpace(name))
        {
            var category = new Category
            {
                Name = name.Trim()
            };
            await store.SaveCategoryAsync(category);
            await this.LoadCategories();
        }
    }
}
