using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Application.UseCases;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class DishCatalogViewModel : ViewModelBase
{
    private readonly CreateDishHandler createDishHandler;
    private readonly GetDishesHandler getDishesHandler;
    private readonly GetProductsHandler getProductsHandler;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private ProductListItemViewModel? selectedProduct;

    [ObservableProperty]
    private decimal ingredientWeight = 100;

    [ObservableProperty]
    private string statusMessage = "Добавьте ингредиенты и создайте блюдо.";

    public DishCatalogViewModel(
        CreateDishHandler createDishHandler,
        GetDishesHandler getDishesHandler,
        GetProductsHandler getProductsHandler)
    {
        this.createDishHandler = createDishHandler;
        this.getDishesHandler = getDishesHandler;
        this.getProductsHandler = getProductsHandler;
    }

    public ObservableCollection<ProductListItemViewModel> AvailableProducts { get; } = [];

    public ObservableCollection<DishIngredientSelectionViewModel> SelectedIngredients { get; } = [];

    public ObservableCollection<DishListItemViewModel> Dishes { get; } = [];

    public async Task InitializeAsync()
    {
        await ReloadReferenceDataAsync();
        await ReloadDishesAsync();
    }

    public async Task ReloadReferenceDataAsync()
    {
        var products = await getProductsHandler.Handle(new GetProductsQuery());
        var selectedProductId = SelectedProduct?.Id;

        AvailableProducts.Clear();

        foreach (var product in products)
        {
            AvailableProducts.Add(new ProductListItemViewModel(product));
        }

        SelectedProduct = selectedProductId is null
            ? AvailableProducts.FirstOrDefault()
            : AvailableProducts.FirstOrDefault(product => product.Id == selectedProductId) ?? AvailableProducts.FirstOrDefault();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await ReloadReferenceDataAsync();
        await ReloadDishesAsync();
        StatusMessage = "Справочники и блюда обновлены.";
    }

    [RelayCommand]
    private void AddIngredient()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Сначала добавьте хотя бы один продукт в справочник.";
            return;
        }

        if (IngredientWeight <= 0)
        {
            StatusMessage = "Вес ингредиента должен быть больше нуля.";
            return;
        }

        var existingIngredient = SelectedIngredients.FirstOrDefault(ingredient => ingredient.ProductId == SelectedProduct.Id);
        if (existingIngredient is not null)
        {
            existingIngredient.IncreaseWeight(IngredientWeight);
            RebuildSelectedIngredients();
        }
        else
        {
            SelectedIngredients.Add(new DishIngredientSelectionViewModel(
                SelectedProduct.Id,
                SelectedProduct.Name,
                IngredientWeight));
        }

        StatusMessage = $"Ингредиент \"{SelectedProduct.Name}\" добавлен.";
        IngredientWeight = 100;
    }

    [RelayCommand]
    private async Task CreateDish()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "Укажите название блюда.";
            return;
        }

        if (SelectedIngredients.Count == 0)
        {
            StatusMessage = "Добавьте хотя бы один ингредиент.";
            return;
        }

        var dishName = Name.Trim();

        await createDishHandler.Handle(new CreateDishCommand
        {
            Name = dishName,
            Ingredients = SelectedIngredients
                .Select(ingredient => new CreateDishIngredientModel
                {
                    ProductId = ingredient.ProductId,
                    Weight = ingredient.Weight
                })
                .ToList()
        });

        await ReloadDishesAsync();

        Name = string.Empty;
        IngredientWeight = 100;
        SelectedIngredients.Clear();
        StatusMessage = $"Блюдо \"{dishName}\" создано.";
    }

    private async Task ReloadDishesAsync()
    {
        var dishes = await getDishesHandler.Handle(new GetDishesQuery());

        Dishes.Clear();

        foreach (var dish in dishes)
        {
            Dishes.Add(new DishListItemViewModel(dish));
        }
    }

    private void RebuildSelectedIngredients()
    {
        var snapshot = SelectedIngredients.ToList();
        SelectedIngredients.Clear();

        foreach (var ingredient in snapshot)
        {
            SelectedIngredients.Add(ingredient);
        }
    }
}
