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

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly CreateRationHandler createRationHandler;
    private readonly GetRationsHandler getRationsHandler;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? startDate = DateTimeOffset.Now;

    [ObservableProperty]
    private int durationDays = 3;

    [ObservableProperty]
    private int participantCount = 2;

    [ObservableProperty]
    private TourismType selectedTourismType = TourismType.Hiking;

    [ObservableProperty]
    private Season selectedSeason = Season.Summer;

    [ObservableProperty]
    private string statusMessage = "Введите параметры проекта рациона и сохраните его.";

    [ObservableProperty]
    private RationProjectListItemViewModel? selectedRationProject;

    public MainWindowViewModel(
        CreateRationHandler createRationHandler,
        GetRationsHandler getRationsHandler)
    {
        this.createRationHandler = createRationHandler;
        this.getRationsHandler = getRationsHandler;
    }

    public ObservableCollection<RationProjectListItemViewModel> RationProjects { get; } = [];

    public IReadOnlyList<TourismType> TourismTypes { get; } = Enum.GetValues<TourismType>();

    public IReadOnlyList<Season> Seasons { get; } = Enum.GetValues<Season>();

    [RelayCommand]
    private async Task CreateRation()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            StatusMessage = "Укажите название рациона.";
            return;
        }

        if (DurationDays <= 0)
        {
            StatusMessage = "Длительность должна быть больше нуля.";
            return;
        }

        if (ParticipantCount <= 0)
        {
            StatusMessage = "Количество участников должно быть больше нуля.";
            return;
        }

        var rationName = Name.Trim();

        await createRationHandler.Handle(new CreateRationCommand
        {
            Name = rationName,
            StartDate = StartDate?.DateTime ?? DateTime.Today,
            DurationDays = DurationDays,
            ParticipantCount = ParticipantCount,
            TourismType = SelectedTourismType,
            Season = SelectedSeason
        });

        await ReloadProjects();

        SelectedRationProject = RationProjects.FirstOrDefault(project => project.Name == rationName);
        StatusMessage = $"Рацион \"{rationName}\" сохранен.";

        Name = string.Empty;
        StartDate = DateTimeOffset.Now;
        DurationDays = 3;
        ParticipantCount = 2;
        SelectedTourismType = TourismType.Hiking;
        SelectedSeason = Season.Summer;
    }

    public async Task InitializeAsync()
    {
        await ReloadProjects();
    }

    [RelayCommand]
    private async Task LoadRations()
    {
        await ReloadProjects();
        StatusMessage = $"Загружено рационов: {RationProjects.Count}.";
    }

    private async Task ReloadProjects()
    {
        var projects = await getRationsHandler.Handle(new GetRationsQuery());

        RationProjects.Clear();

        foreach (var project in projects)
        {
            RationProjects.Add(new RationProjectListItemViewModel(project));
        }
    }
}
