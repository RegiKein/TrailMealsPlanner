using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Application.UseCases;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class ProductCatalogViewModel : ViewModelBase
{
    private readonly CreateProductHandler createProductHandler;
    private readonly GetProductsHandler getProductsHandler;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private decimal caloriesPer100g;

    [ObservableProperty]
    private decimal protein;

    [ObservableProperty]
    private decimal fat;

    [ObservableProperty]
    private decimal carbs;

    [ObservableProperty]
    private string statusMessage = "Добавьте продукт в справочник.";

    public ProductCatalogViewModel(
        CreateProductHandler createProductHandler,
        GetProductsHandler getProductsHandler)
    {
        this.createProductHandler = createProductHandler;
        this.getProductsHandler = getProductsHandler;
    }

    public event EventHandler? ProductsChanged;

    public ObservableCollection<ProductListItemViewModel> Products { get; } = [];

    public async Task InitializeAsync()
    {
        await ReloadProducts();
    }

    [RelayCommand]
    private async Task CreateProduct()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "Укажите название продукта.";
            return;
        }

        var productName = Name.Trim();

        await createProductHandler.Handle(new CreateProductCommand
        {
            Name = productName,
            CaloriesPer100g = CaloriesPer100g,
            Protein = Protein,
            Fat = Fat,
            Carbs = Carbs
        });

        await ReloadProducts();

        StatusMessage = $"Продукт \"{productName}\" добавлен.";
        Name = string.Empty;
        CaloriesPer100g = 0;
        Protein = 0;
        Fat = 0;
        Carbs = 0;

        ProductsChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task ReloadProducts()
    {
        var products = await getProductsHandler.Handle(new GetProductsQuery());

        Products.Clear();

        foreach (var product in products)
        {
            Products.Add(new ProductListItemViewModel(product));
        }
    }
}
