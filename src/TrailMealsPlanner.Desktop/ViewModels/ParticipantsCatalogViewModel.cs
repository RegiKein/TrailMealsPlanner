using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class ParticipantsCatalogViewModel : ViewModelBase
{
    private readonly AddParticipantHandler addParticipantHandler;
    private readonly GetParticipantsHandler getParticipantsHandler;
    private readonly GetProductPreferencesHandler getProductPreferencesHandler;
    private readonly GetProductsHandler getProductsHandler;
    private readonly SetProductPreferenceHandler setProductPreferenceHandler;

    [ObservableProperty]
    private string participantName = string.Empty;

    [ObservableProperty]
    private ParticipantListItemViewModel? selectedParticipant;

    [ObservableProperty]
    private ProductListItemViewModel? selectedProduct;

    [ObservableProperty]
    private PreferenceLevelOptionViewModel? selectedPreferenceLevel;

    [ObservableProperty]
    private string comment = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Добавьте участника и отметьте продукты с аллергией или неприязнью.";

    public ParticipantsCatalogViewModel(
        AddParticipantHandler addParticipantHandler,
        GetParticipantsHandler getParticipantsHandler,
        GetProductsHandler getProductsHandler,
        SetProductPreferenceHandler setProductPreferenceHandler,
        GetProductPreferencesHandler getProductPreferencesHandler)
    {
        this.addParticipantHandler = addParticipantHandler;
        this.getParticipantsHandler = getParticipantsHandler;
        this.getProductsHandler = getProductsHandler;
        this.setProductPreferenceHandler = setProductPreferenceHandler;
        this.getProductPreferencesHandler = getProductPreferencesHandler;

        PreferenceLevels =
        [
            new PreferenceLevelOptionViewModel(PreferenceLevel.Allergy, "Аллергия"),
            new PreferenceLevelOptionViewModel(PreferenceLevel.Dislike, "Не любит"),
            new PreferenceLevelOptionViewModel(PreferenceLevel.Neutral, "Нейтрально"),
            new PreferenceLevelOptionViewModel(PreferenceLevel.Like, "Нравится")
        ];
        SelectedPreferenceLevel = PreferenceLevels[2];
    }

    public event EventHandler? PreferencesChanged;

    public ObservableCollection<ParticipantListItemViewModel> Participants { get; } = [];

    public ObservableCollection<ProductListItemViewModel> Products { get; } = [];

    public ObservableCollection<ProductPreferenceListItemViewModel> Preferences { get; } = [];

    public IReadOnlyList<PreferenceLevelOptionViewModel> PreferenceLevels { get; }

    public async Task InitializeAsync()
    {
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task AddParticipant()
    {
        if (string.IsNullOrWhiteSpace(ParticipantName))
        {
            StatusMessage = "Укажите имя участника.";
            return;
        }

        var name = ParticipantName.Trim();
        await addParticipantHandler.Handle(new AddParticipantCommand { Name = name });
        ParticipantName = string.Empty;
        await ReloadAsync();
        StatusMessage = $"Участник \"{name}\" добавлен.";
        PreferencesChanged?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task SavePreference()
    {
        if (SelectedParticipant is null)
        {
            StatusMessage = "Выберите участника.";
            return;
        }

        if (SelectedProduct is null)
        {
            StatusMessage = "Выберите продукт.";
            return;
        }

        if (SelectedPreferenceLevel is null)
        {
            StatusMessage = "Выберите уровень предпочтения.";
            return;
        }

        await setProductPreferenceHandler.Handle(new SetProductPreferenceCommand
        {
            ParticipantId = SelectedParticipant.Id,
            ProductId = SelectedProduct.Id,
            PreferenceLevel = SelectedPreferenceLevel.Value,
            Comment = Comment
        });

        await ReloadPreferences();
        StatusMessage = $"Предпочтение сохранено: {SelectedParticipant.Name} -> {SelectedProduct.Name}.";
        Comment = string.Empty;
        PreferencesChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task ReloadReferenceDataAsync()
    {
        await ReloadParticipants();
        await ReloadProducts();
        await ReloadPreferences();
    }

    private async Task ReloadAsync()
    {
        await ReloadParticipants();
        await ReloadProducts();
        await ReloadPreferences();
    }

    private async Task ReloadParticipants()
    {
        var participants = await getParticipantsHandler.Handle(new GetParticipantsQuery());
        var selectedId = SelectedParticipant?.Id;

        Participants.Clear();
        foreach (var participant in participants)
        {
            Participants.Add(new ParticipantListItemViewModel(participant));
        }

        SelectedParticipant = selectedId is null
            ? Participants.FirstOrDefault()
            : Participants.FirstOrDefault(item => item.Id == selectedId.Value) ?? Participants.FirstOrDefault();
    }

    private async Task ReloadProducts()
    {
        var products = await getProductsHandler.Handle(new GetProductsQuery());
        var selectedId = SelectedProduct?.Id;

        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(new ProductListItemViewModel(product));
        }

        SelectedProduct = selectedId is null
            ? Products.FirstOrDefault()
            : Products.FirstOrDefault(item => item.Id == selectedId.Value) ?? Products.FirstOrDefault();
    }

    private async Task ReloadPreferences()
    {
        var preferences = await getProductPreferencesHandler.Handle(new GetProductPreferencesQuery());
        Preferences.Clear();

        foreach (var preference in preferences)
        {
            Preferences.Add(new ProductPreferenceListItemViewModel(preference));
        }
    }
}
