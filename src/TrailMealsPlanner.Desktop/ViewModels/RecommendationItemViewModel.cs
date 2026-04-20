using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class RecommendationItemViewModel
{
    public RecommendationItemViewModel(RecommendationDto recommendation)
    {
        Message = recommendation.Message;
        Priority = recommendation.Priority;
    }

    public string Message { get; }

    public int Priority { get; }
}
