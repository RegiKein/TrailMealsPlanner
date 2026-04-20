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
    private readonly ApplyDayTemplateHandler applyDayTemplateHandler;
    private readonly CopyDayHandler copyDayHandler;
    private readonly CopyMealHandler copyMealHandler;
    private readonly ExportRationHandler exportRationHandler;
    private readonly GetDayTemplatesHandler getDayTemplatesHandler;
    private readonly GetDishesHandler getDishesHandler;
    private readonly GetRationAnalyticsHandler getRationAnalyticsHandler;
    private readonly GetRationByIdHandler getRationByIdHandler;
    private readonly SaveDayAsTemplateHandler saveDayAsTemplateHandler;
    private IReadOnlyList<DishOptionViewModel> availableDishes = [];
    private IReadOnlyList<DayCopyTargetViewModel> availableCopyTargets = [];
    private IReadOnlyList<MealCopyTargetViewModel> availableMealCopyTargets = [];
    private IReadOnlyList<DayTemplateOptionViewModel> availableTemplates = [];

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public RationDetailsViewModel(
        GetRationByIdHandler getRationByIdHandler,
        GetRationAnalyticsHandler getRationAnalyticsHandler,
        GetDishesHandler getDishesHandler,
        AddDishToMealHandler addDishToMealHandler,
        CopyDayHandler copyDayHandler,
        CopyMealHandler copyMealHandler,
        ExportRationHandler exportRationHandler,
        GetDayTemplatesHandler getDayTemplatesHandler,
        SaveDayAsTemplateHandler saveDayAsTemplateHandler,
        ApplyDayTemplateHandler applyDayTemplateHandler)
    {
        this.getRationByIdHandler = getRationByIdHandler;
        this.getRationAnalyticsHandler = getRationAnalyticsHandler;
        this.getDishesHandler = getDishesHandler;
        this.addDishToMealHandler = addDishToMealHandler;
        this.copyDayHandler = copyDayHandler;
        this.copyMealHandler = copyMealHandler;
        this.exportRationHandler = exportRationHandler;
        this.getDayTemplatesHandler = getDayTemplatesHandler;
        this.saveDayAsTemplateHandler = saveDayAsTemplateHandler;
        this.applyDayTemplateHandler = applyDayTemplateHandler;
    }

    public Guid RationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public NutritionSummaryViewModel Analytics { get; private set; } = new(new NutritionInfoDto());

    public ObservableCollection<RationDayViewModel> Days { get; } = [];

    public ObservableCollection<DayTemplateOptionViewModel> Templates { get; } = [];

    public bool HasRation => RationId != Guid.Empty;

    public bool IsEmpty => !HasRation;

    public bool HasTemplates => Templates.Count > 0;

    public string ExportDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "TrailMealsPlanner",
        "Exports");

    public async Task LoadAsync(Guid rationId, CancellationToken cancellationToken = default)
    {
        availableDishes = await LoadDishOptions(cancellationToken);
        availableTemplates = await LoadTemplateOptions(cancellationToken);

        var details = await getRationByIdHandler.Handle(
            new GetRationByIdQuery { Id = rationId },
            cancellationToken);
        var analytics = await getRationAnalyticsHandler.Handle(
            new GetRationAnalyticsQuery { RationId = rationId },
            cancellationToken);

        Days.Clear();
        Templates.Clear();

        foreach (var template in availableTemplates)
        {
            Templates.Add(template);
        }

        OnPropertyChanged(nameof(HasTemplates));

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
        availableMealCopyTargets = details.Days
            .SelectMany(day => day.Meals.Select(meal => new MealCopyTargetViewModel(meal.Id, day.DayNumber, day.Date, meal.Type)))
            .ToList();

        foreach (var day in details.Days)
        {
            var dayAnalytics = analytics?.Days.FirstOrDefault(item => item.DayId == day.Id);
            var dayViewModel = new RationDayViewModel(
                day,
                dayAnalytics,
                availableDishes,
                availableMealCopyTargets,
                AddDishToMealAsync,
                targetDayId => CopyDayAsync(day.Id, targetDayId),
                CopyMealAsync,
                (dayId, templateName) => SaveDayAsTemplateAsync(dayId, templateName),
                ApplyDayTemplateAsync);
            dayViewModel.UpdateCopyTargets(availableCopyTargets);
            dayViewModel.UpdateMealCopyTargets();
            dayViewModel.UpdateTemplateOptions(availableTemplates);
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

    private async Task CopyMealAsync(Guid sourceMealId, Guid targetMealId)
    {
        if (RationId == Guid.Empty)
        {
            return;
        }

        await copyMealHandler.Handle(new CopyMealCommand
        {
            RationId = RationId,
            SourceMealId = sourceMealId,
            TargetMealId = targetMealId
        });

        await LoadAsync(RationId);
        StatusMessage = "Приём пищи скопирован.";
    }

    private async Task SaveDayAsTemplateAsync(Guid dayId, string templateName)
    {
        if (RationId == Guid.Empty)
        {
            return;
        }

        await saveDayAsTemplateHandler.Handle(new SaveDayAsTemplateCommand
        {
            RationId = RationId,
            DayId = dayId,
            Name = templateName
        });

        await LoadAsync(RationId);
        StatusMessage = $"Шаблон \"{templateName}\" сохранён.";
    }

    private async Task ApplyDayTemplateAsync(Guid templateId, Guid targetDayId)
    {
        if (RationId == Guid.Empty)
        {
            return;
        }

        var templateName = availableTemplates.FirstOrDefault(template => template.Id == templateId)?.Name ?? "Шаблон";

        await applyDayTemplateHandler.Handle(new ApplyDayTemplateCommand
        {
            RationId = RationId,
            TemplateId = templateId,
            TargetDayId = targetDayId
        });

        await LoadAsync(RationId);
        StatusMessage = $"Шаблон \"{templateName}\" применён к дню.";
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

    private async Task<IReadOnlyList<DayTemplateOptionViewModel>> LoadTemplateOptions(CancellationToken cancellationToken)
    {
        var templates = await getDayTemplatesHandler.Handle(new GetDayTemplatesQuery(), cancellationToken);
        return templates.Select(template => new DayTemplateOptionViewModel(template)).ToList();
    }
}
