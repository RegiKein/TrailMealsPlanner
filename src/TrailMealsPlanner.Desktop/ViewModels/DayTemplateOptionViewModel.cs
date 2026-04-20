using System;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class DayTemplateOptionViewModel
{
    public DayTemplateOptionViewModel(DayTemplateListItemDto template)
    {
        Id = template.Id;
        Name = template.Name;
        CreatedAt = template.CreatedAt;
        MealsCount = template.MealsCount;
        ItemsCount = template.ItemsCount;
        Summary = template.Summary;
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTime CreatedAt { get; }

    public int MealsCount { get; }

    public int ItemsCount { get; }

    public string Summary { get; }

    public string Display => $"{Name} ({Summary})";
}
