namespace TrailMealsPlanner.Application.UseCases;

public sealed class CopyDayCommand
{
    public Guid RationId { get; init; }

    public Guid SourceDayId { get; init; }

    public Guid TargetDayId { get; init; }
}
