namespace TrailMealsPlanner.Application.UseCases;

public sealed class ApplyDayTemplateCommand
{
    public Guid RationId { get; init; }

    public Guid TemplateId { get; init; }

    public Guid TargetDayId { get; init; }
}
