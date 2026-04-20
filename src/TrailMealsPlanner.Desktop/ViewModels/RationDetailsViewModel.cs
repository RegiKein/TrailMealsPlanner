using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.UseCases;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed partial class RationDetailsViewModel : ViewModelBase
{
    private readonly AddDishToMealHandler addDishToMealHandler;
    private readonly CopyDayHandler copyDayHandler;
    private readonly ExportRationHandler exportRationHandler;
    private readonly GetDishesHandler getDishesHandler;
    private readonly GetRationAnalyticsHandler getRationAnalyticsHandler;
    private readonly GetRationByIdHandler getRationByIdHandler;
    private IReadOnlyList<DishOptionViewModel> availableDishes = [];
    private IReadOnlyList<DayCopyTargetViewModel> availableCopyTargets = [];

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public RationDetailsViewModel(
        GetRationByIdHandler getRationByIdHandler,
        GetRationAnalyticsHandler getRationAnalyticsHandler,
        GetDishesHandler getDishesHandler,
        AddDishToMealHandler addDishToMealHandler,
        CopyDayHandler copyDayHandler,
        ExportRationHandler exportRationHandler)
    {
        this.getRationByIdHandler = getRationByIdHandler;
        this.getRationAnalyticsHandler = getRationAnalyticsHandler;
        this.getDishesHandler = getDishesHandler;
        this.addDishToMealHandler = addDishToMealHandler;
        this.copyDayHandler = copyDayHandler;
        this.exportRationHandler = exportRationHandler;
    }

    public Guid RationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public NutritionSummaryViewModel Analytics { get; private set; } = new(new NutritionInfoDto());

    public ObservableCollection<RationDayViewModel> Days { get; } = [];

    public bool HasRation => RationId != Guid.Empty;

    public bool IsEmpty => !HasRation;

    public string ExportDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "TrailMealsPlanner",
        "Exports");

    public async Task LoadAsync(Guid rationId, CancellationToken cancellationToken = default)
    {
        availableDishes = await LoadDishOptions(cancellationToken);

        var details = await getRationByIdHandler.Handle(
            new GetRationByIdQuery { Id = rationId },
            cancellationToken);
        var analytics = await getRationAnalyticsHandler.Handle(
            new GetRationAnalyticsQuery { RationId = rationId },
            cancellationToken);

        Days.Clear();

        if (details is null)
        {
            RationId = Guid.Empty;
            Name = string.Empty;
            Analytics = new NutritionSummaryViewModel(new NutritionInfoDto());
            StatusMessage = string.Empty;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Analytics));
            OnPropertyChanged(nameof(HasRation));
            OnPropertyChanged(nameof(IsEmpty));
            return;
        }

        RationId = details.Id;
        Name = details.Name;
        Analytics = analytics is null
            ? new NutritionSummaryViewModel(new NutritionInfoDto())
            : new NutritionSummaryViewModel(analytics.Nutrition);
        availableCopyTargets = details.Days
            .Select(day => new DayCopyTargetViewModel(day.Id, day.DayNumber, day.Date))
            .ToList();

        foreach (var day in details.Days)
        {
            var dayAnalytics = analytics?.Days.FirstOrDefault(item => item.DayId == day.Id);
            var dayViewModel = new RationDayViewModel(
                day,
                dayAnalytics,
                availableDishes,
                AddDishToMealAsync,
                targetDayId => CopyDayAsync(day.Id, targetDayId));
            dayViewModel.UpdateCopyTargets(availableCopyTargets);
            Days.Add(dayViewModel);
        }

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Analytics));
        OnPropertyChanged(nameof(HasRation));
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private async Task ExportJson()
    {
        await ExportAsync(RationExportFormat.Json);
    }

    [RelayCommand]
    private async Task ExportText()
    {
        await ExportAsync(RationExportFormat.Text);
    }

    [RelayCommand]
    private async Task ExportPdf()
    {
        await ExportAsync(RationExportFormat.Pdf);
    }

    private async Task AddDishToMealAsync(Guid mealId, Guid dishId, decimal quantity)
    {
        if (RationId == Guid.Empty)
        {
            return;
        }

        await addDishToMealHandler.Handle(new AddDishToMealCommand
        {
            RationId = RationId,
            MealId = mealId,
            DishId = dishId,
            Quantity = quantity
        });

        await LoadAsync(RationId);
        StatusMessage = "Блюдо добавлено в рацион.";
    }

    private async Task CopyDayAsync(Guid sourceDayId, Guid targetDayId)
    {
        if (RationId == Guid.Empty)
        {
            return;
        }

        await copyDayHandler.Handle(new CopyDayCommand
        {
            RationId = RationId,
            SourceDayId = sourceDayId,
            TargetDayId = targetDayId
        });

        await LoadAsync(RationId);
        StatusMessage = "День скопирован.";
    }

    private async Task ExportAsync(RationExportFormat format)
    {
        if (RationId == Guid.Empty)
        {
            StatusMessage = "Сначала выберите рацион.";
            return;
        }

        var path = await exportRationHandler.Handle(new ExportRationCommand
        {
            RationId = RationId,
            Format = format,
            DestinationDirectory = ExportDirectory
        });

        StatusMessage = $"Файл сохранён: {path}";
    }

    private async Task<IReadOnlyList<DishOptionViewModel>> LoadDishOptions(CancellationToken cancellationToken)
    {
        var dishes = await getDishesHandler.Handle(new GetDishesQuery(), cancellationToken);
        return dishes.Select(dish => new DishOptionViewModel(dish)).ToList();
    }
}
