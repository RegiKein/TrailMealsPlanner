namespace TrailMealsPlanner.Application.UseCases;

public sealed class CopyMealCommand
{
    public Guid RationId { get; init; }

    public Guid SourceMealId { get; init; }

    public Guid TargetMealId { get; init; }
}
