using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TrailMealsPlanner.Application.UseCases;

namespace TrailMealsPlanner.Desktop.ViewModels;

public partial class DayTemplatesViewModel : ViewModelBase
{
    private readonly GetDayTemplatesHandler getDayTemplatesHandler;

    [ObservableProperty]
    private string statusMessage = "Шаблоны дней помогают повторно использовать типовые структуры.";

    public DayTemplatesViewModel(GetDayTemplatesHandler getDayTemplatesHandler)
    {
        this.getDayTemplatesHandler = getDayTemplatesHandler;
    }

    public ObservableCollection<DayTemplateOptionViewModel> Templates { get; } = [];

    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        var templates = await getDayTemplatesHandler.Handle(new GetDayTemplatesQuery());
        Templates.Clear();
        foreach (var template in templates)
        {
            Templates.Add(new DayTemplateOptionViewModel(template));
        }

        StatusMessage = Templates.Count == 0
            ? "Пока нет сохранённых шаблонов дней."
            : $"Загружено шаблонов: {Templates.Count}.";
    }
}
