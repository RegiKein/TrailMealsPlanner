using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class RationProject
{
    private readonly List<RationDay> days = [];

    public RationProject(
        string name,
        DateTime startDate,
        int durationDays,
        int participantCount,
        RationProfile profile)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Ration project name is required.", nameof(name));
        }

        if (durationDays <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationDays), "Duration must be greater than zero.");
        }

        if (participantCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(participantCount), "Participant count must be greater than zero.");
        }

        Profile = profile ?? throw new ArgumentNullException(nameof(profile));

        Id = Guid.NewGuid();
        Name = name.Trim();
        StartDate = startDate.Date;
        DurationDays = durationDays;
        ParticipantCount = participantCount;
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTime StartDate { get; }

    public DateTime EndDate => StartDate.AddDays(DurationDays - 1);

    public int DurationDays { get; }

    public int ParticipantCount { get; }

    public RationProfile Profile { get; }

    public IReadOnlyList<RationDay> Days => days;

    public void AddDishToMeal(Guid mealId, Guid dishId, decimal quantity)
    {
        var day = days.FirstOrDefault(item => item.Meals.Any(meal => meal.Id == mealId));
        if (day is null)
        {
            throw new InvalidOperationException($"Meal '{mealId}' was not found in ration project '{Id}'.");
        }

        day.AddDishToMeal(mealId, dishId, quantity);
    }

    public void ReplaceDayContent(Guid sourceDayId, Guid targetDayId)
    {
        if (sourceDayId == Guid.Empty)
        {
            throw new ArgumentException("Source day id is required.", nameof(sourceDayId));
        }

        if (targetDayId == Guid.Empty)
        {
            throw new ArgumentException("Target day id is required.", nameof(targetDayId));
        }

        if (sourceDayId == targetDayId)
        {
            throw new InvalidOperationException("Source and target day must be different.");
        }

        var sourceDay = days.FirstOrDefault(day => day.Id == sourceDayId);
        if (sourceDay is null)
        {
            throw new InvalidOperationException($"Source day '{sourceDayId}' was not found in ration project '{Id}'.");
        }

        var targetIndex = days.FindIndex(day => day.Id == targetDayId);
        if (targetIndex < 0)
        {
            throw new InvalidOperationException($"Target day '{targetDayId}' was not found in ration project '{Id}'.");
        }

        var targetDay = days[targetIndex];
        days[targetIndex] = sourceDay.CloneTo(targetDay.Date, targetDay.DayNumber);
    }

    public void ReplaceMealContent(Guid sourceMealId, Guid targetMealId)
    {
        if (sourceMealId == Guid.Empty)
        {
            throw new ArgumentException("Source meal id is required.", nameof(sourceMealId));
        }

        if (targetMealId == Guid.Empty)
        {
            throw new ArgumentException("Target meal id is required.", nameof(targetMealId));
        }

        if (sourceMealId == targetMealId)
        {
            throw new InvalidOperationException("Source and target meal must be different.");
        }

        var sourceMeal = days
            .SelectMany(day => day.Meals)
            .FirstOrDefault(meal => meal.Id == sourceMealId);
        if (sourceMeal is null)
        {
            throw new InvalidOperationException($"Source meal '{sourceMealId}' was not found in ration project '{Id}'.");
        }

        var targetMeal = days
            .SelectMany(day => day.Meals)
            .FirstOrDefault(meal => meal.Id == targetMealId);
        if (targetMeal is null)
        {
            throw new InvalidOperationException($"Target meal '{targetMealId}' was not found in ration project '{Id}'.");
        }

        targetMeal.ReplaceContentFrom(sourceMeal);
    }

    public void GenerateDays()
    {
        days.Clear();

        for (var dayNumber = 1; dayNumber <= DurationDays; dayNumber++)
        {
            var day = new RationDay(Id, StartDate.AddDays(dayNumber - 1), dayNumber);
            day.InitializeDefaultMeals();
            days.Add(day);
        }
    }

    public NutritionInfo CalculateNutrition(
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById)
    {
        return days.Aggregate(
            NutritionInfo.Zero,
            (current, day) => current.Add(day.CalculateNutrition(dishesById, productsById)));
    }
}
