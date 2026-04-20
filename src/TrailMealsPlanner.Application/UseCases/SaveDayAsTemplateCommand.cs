namespace TrailMealsPlanner.Application.UseCases;

public sealed class SaveDayAsTemplateCommand
{
    public Guid RationId { get; init; }

    public Guid DayId { get; init; }

    public string Name { get; init; } = string.Empty;
}
