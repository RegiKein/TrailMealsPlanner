using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class ProductPreferenceListItemViewModel
{
    public ProductPreferenceListItemViewModel(ProductPreferenceDto preference)
    {
        ParticipantName = preference.ParticipantName;
        ProductName = preference.ProductName;
        PreferenceLevel = preference.PreferenceLevel;
        Comment = preference.Comment;
    }

    public string ParticipantName { get; }

    public string ProductName { get; }

    public PreferenceLevel PreferenceLevel { get; }

    public string? Comment { get; }

    public string Summary => string.IsNullOrWhiteSpace(Comment)
        ? $"{ParticipantName} -> {ProductName}: {GetLabel(PreferenceLevel)}"
        : $"{ParticipantName} -> {ProductName}: {GetLabel(PreferenceLevel)} ({Comment})";

    private static string GetLabel(PreferenceLevel level)
    {
        return level switch
        {
            PreferenceLevel.Allergy => "Аллергия",
            PreferenceLevel.Dislike => "Не любит",
            PreferenceLevel.Neutral => "Нейтрально",
            PreferenceLevel.Like => "Нравится",
            _ => level.ToString()
        };
    }
}
