using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.Services;

public sealed class GroupPreferenceAnalysisService : IGroupPreferenceAnalysisService
{
    public FoodIssueDto? AnalyzeMealItem(
        MealItem mealItem,
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById,
        IReadOnlyList<Participant> participants,
        IReadOnlyList<ProductPreference> preferences)
    {
        var productIds = ResolveProductIds(mealItem, dishesById);
        if (productIds.Count == 0)
        {
            return null;
        }

        var participantNamesById = participants.ToDictionary(participant => participant.Id, participant => participant.Name);
        var relevantPreferences = preferences
            .Where(preference => productIds.Contains(preference.ProductId))
            .ToList();

        var allergyParticipants = relevantPreferences
            .Where(preference => preference.PreferenceLevel == PreferenceLevel.Allergy)
            .Select(preference => participantNamesById.GetValueOrDefault(preference.ParticipantId))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        if (allergyParticipants.Count > 0)
        {
            return new FoodIssueDto
            {
                Severity = PreferenceLevel.Allergy,
                Summary = $"Аллергия: {string.Join(", ", allergyParticipants)}"
            };
        }

        var dislikeParticipants = relevantPreferences
            .Where(preference => preference.PreferenceLevel == PreferenceLevel.Dislike)
            .Select(preference => participantNamesById.GetValueOrDefault(preference.ParticipantId))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(name => name)
            .ToList();

        if (dislikeParticipants.Count >= GetMassDislikeThreshold(participants.Count))
        {
            return new FoodIssueDto
            {
                Severity = PreferenceLevel.Dislike,
                Summary = $"Нежелательно для группы: {string.Join(", ", dislikeParticipants)}"
            };
        }

        return null;
    }

    private static HashSet<Guid> ResolveProductIds(MealItem mealItem, IReadOnlyDictionary<Guid, Dish> dishesById)
    {
        if (mealItem.ProductId is Guid productId)
        {
            return [productId];
        }

        if (mealItem.DishId is Guid dishId && dishesById.TryGetValue(dishId, out var dish))
        {
            return dish.Ingredients.Select(ingredient => ingredient.ProductId).ToHashSet();
        }

        return [];
    }

    private static int GetMassDislikeThreshold(int participantCount)
    {
        if (participantCount <= 1)
        {
            return 1;
        }

        return Math.Max(2, (int)Math.Ceiling(participantCount / 2m));
    }
}
