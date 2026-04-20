using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Desktop.Services;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DayTemplatesViewModel dayTemplates;
    private readonly DishCatalogViewModel dishes;
    private readonly ParticipantsCatalogViewModel participants;
    private readonly ProductCatalogViewModel products;
    private readonly RationDetailsViewModel rationDetails;
    private readonly RationListViewModel rationList;
    private readonly SettingsViewModel settings;
    private readonly LocalizationService localizationService;

    [ObservableProperty]
    private ViewModelBase currentScreen;

    [ObservableProperty]
    private string currentScreenTitle;

    public MainWindowViewModel(
        RationListViewModel rationList,
        RationDetailsViewModel rationDetails,
        ProductCatalogViewModel products,
        DishCatalogViewModel dishes,
        ParticipantsCatalogViewModel participants,
        DayTemplatesViewModel dayTemplates,
        SettingsViewModel settings,
        LocalizationService localizationService)
    {
        this.rationList = rationList;
        this.rationDetails = rationDetails;
        this.products = products;
        this.dishes = dishes;
        this.participants = participants;
        this.dayTemplates = dayTemplates;
        this.settings = settings;
        this.localizationService = localizationService;

        currentScreen = rationList;
        currentScreenTitle = GetTitleFor(rationList);

        rationList.RationRequested += OnRationRequested;
        participants.PreferencesChanged += OnParticipantsChanged;
        products.ProductsChanged += OnProductsChanged;
        localizationService.CultureChanged += OnCultureChanged;
    }

    public string WindowTitle => localizationService.Get("Window_Title");

    public async Task InitializeAsync()
    {
        await rationList.InitializeAsync();
        await products.InitializeAsync();
        await dishes.InitializeAsync();
        await participants.InitializeAsync();
        await dayTemplates.InitializeAsync();
    }

    [RelayCommand]
    private void ShowRations()
    {
        CurrentScreen = rationList;
        CurrentScreenTitle = GetTitleFor(rationList);
    }

    [RelayCommand]
    private void ShowRationDetails()
    {
        if (!rationDetails.HasRation)
        {
            ShowRations();
            return;
        }

        CurrentScreen = rationDetails;
        CurrentScreenTitle = GetTitleFor(rationDetails);
    }

    [RelayCommand]
    private void ShowProducts()
    {
        CurrentScreen = products;
        CurrentScreenTitle = GetTitleFor(products);
    }

    [RelayCommand]
    private void ShowDishes()
    {
        CurrentScreen = dishes;
        CurrentScreenTitle = GetTitleFor(dishes);
    }

    [RelayCommand]
    private void ShowPreferences()
    {
        CurrentScreen = participants;
        CurrentScreenTitle = GetTitleFor(participants);
    }

    [RelayCommand]
    private async Task ShowTemplatesAsync()
    {
        await dayTemplates.RefreshAsync();
        CurrentScreen = dayTemplates;
        CurrentScreenTitle = GetTitleFor(dayTemplates);
    }

    [RelayCommand]
    private void ShowSettings()
    {
        CurrentScreen = settings;
        CurrentScreenTitle = GetTitleFor(settings);
    }

    private async void OnRationRequested(object? sender, Guid rationId)
    {
        await rationDetails.LoadAsync(rationId);
        CurrentScreen = rationDetails;
        CurrentScreenTitle = GetTitleFor(rationDetails);
    }

    private async void OnParticipantsChanged(object? sender, EventArgs e)
    {
        if (rationDetails.HasRation)
        {
            await rationDetails.LoadAsync(rationDetails.RationId);
        }
    }

    private async void OnProductsChanged(object? sender, EventArgs e)
    {
        await dishes.ReloadReferenceDataAsync();
        await participants.ReloadReferenceDataAsync();
        if (rationDetails.HasRation)
        {
            await rationDetails.LoadAsync(rationDetails.RationId);
        }
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(WindowTitle));
        CurrentScreenTitle = GetTitleFor(CurrentScreen);
    }

    private string GetTitleFor(ViewModelBase screen)
    {
        return screen switch
        {
            RationListViewModel => localizationService.Get("Navigation_Rations"),
            RationDetailsViewModel => localizationService.Get("Navigation_RationDetails"),
            ProductCatalogViewModel => localizationService.Get("Navigation_Products"),
            DishCatalogViewModel => localizationService.Get("Navigation_Dishes"),
            ParticipantsCatalogViewModel => localizationService.Get("Navigation_Preferences"),
            DayTemplatesViewModel => localizationService.Get("Navigation_DayTemplates"),
            SettingsViewModel => localizationService.Get("Navigation_Settings"),
            _ => localizationService.Get("Window_Title")
        };
    }
}
