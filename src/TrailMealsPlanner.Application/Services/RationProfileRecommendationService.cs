using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.Services;

public sealed class RationProfileRecommendationService : IRationProfileRecommendationService
{
    public IReadOnlyList<RecommendationDto> BuildRecommendations(
        RationProject ration,
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById)
    {
        var recommendations = new List<RecommendationDto>();
        var days = ration.Days.OrderBy(day => day.DayNumber).ToList();
        var dayAnalytics = days
            .Select(day => new DayAnalytics(day, day.CalculateNutrition(dishesById, productsById)))
            .ToList();

        AddActivityRecommendations(recommendations, ration, dayAnalytics);
        AddEnvironmentRecommendations(recommendations, ration);
        AddLogisticsRecommendations(recommendations, ration, dayAnalytics);
        AddContentRecommendations(recommendations, ration, dayAnalytics);

        return recommendations
            .GroupBy(item => item.Message)
            .Select(group => group.OrderBy(item => item.Priority).First())
            .OrderBy(item => item.Priority)
            .ThenBy(item => item.Message)
            .Take(7)
            .ToList();
    }

    private static void AddActivityRecommendations(
        ICollection<RecommendationDto> recommendations,
        RationProject ration,
        IReadOnlyList<DayAnalytics> dayAnalytics)
    {
        switch (ration.Profile.ActivityType)
        {
            case ActivityType.Water:
                recommendations.Add(new RecommendationDto
                {
                    Priority = 10,
                    Message = "Для водного маршрута вес менее критичен: можно оставить более разнообразное и объёмное питание."
                });
                recommendations.Add(new RecommendationDto
                {
                    Priority = 20,
                    Message = "Водный профиль допускает полноценную готовку, поэтому имеет смысл планировать горячие приёмы пищи."
                });
                break;

            case ActivityType.Mountain:
            case ActivityType.Alpine:
                recommendations.Add(new RecommendationDto
                {
                    Priority = 10,
                    Message = "Для горного и альпинистского профиля держите высокую энергоёмкость рациона и минимизируйте лишний вес."
                });

                if (dayAnalytics.Any(day => day.Nutrition.Weight > 850m || day.Nutrition.CaloriesPerGram < 3m))
                {
                    recommendations.Add(new RecommendationDto
                    {
                        Priority = 15,
                        Message = "Есть дни с тяжёлым или слишком \"водянистым\" рационом. Для горных условий лучше повышать ккал/г."
                    });
                }
                break;

            case ActivityType.Speleo:
                recommendations.Add(new RecommendationDto
                {
                    Priority = 10,
                    Message = "Для спелео полезны перекусы и питание на ходу: не завязывайте день только на полноценную готовку."
                });
                break;

            case ActivityType.Hiking:
                recommendations.Add(new RecommendationDto
                {
                    Priority = 20,
                    Message = "Для пешего похода базовый сценарий нормальный: полноценные завтрак, обед и ужин допустимы."
                });
                break;

            case ActivityType.Competition:
                recommendations.Add(new RecommendationDto
                {
                    Priority = 10,
                    Message = "Для соревнований делайте упор на лёгкие, углеводные и быстро употребляемые позиции."
                });
                recommendations.Add(new RecommendationDto
                {
                    Priority = 15,
                    Message = "Соревновательный рацион лучше строить вокруг перекусов и минимального времени на готовку."
                });
                break;

            case ActivityType.WeekendTrip:
            case ActivityType.LeisureWalk:
                recommendations.Add(new RecommendationDto
                {
                    Priority = 25,
                    Message = "Для ПВД и прогулок можно быть менее строгим к весу и добавить больше разнообразия ради комфорта."
                });
                break;

            case ActivityType.Hunting:
                recommendations.Add(new RecommendationDto
                {
                    Priority = 15,
                    Message = "Для охоты важны практичность и автономность: планируйте еду, которая переживает долгие переходы и нерегулярную готовку."
                });
                break;
        }
    }

    private static void AddEnvironmentRecommendations(
        ICollection<RecommendationDto> recommendations,
        RationProject ration)
    {
        if (ration.Profile.Environment.TemperatureRange is TemperatureRange.Cold or TemperatureRange.ExtremeCold)
        {
            recommendations.Add(new RecommendationDto
            {
                Priority = 30,
                Message = "Холодные условия обычно требуют более калорийного рациона и стабильных горячих приёмов пищи."
            });
        }

        if (ration.Profile.Environment.WaterAvailability is WaterAvailability.Scarce or WaterAvailability.None)
        {
            recommendations.Add(new RecommendationDto
            {
                Priority = 25,
                Message = "При дефиците воды лучше избегать продуктов, требующих много воды на готовку и восстановление."
            });
        }

        if (ration.Profile.Environment.AltitudeRange is AltitudeRange.High or AltitudeRange.Extreme)
        {
            recommendations.Add(new RecommendationDto
            {
                Priority = 20,
                Message = "На высоте рациону полезна повышенная энергоёмкость и простота приготовления."
            });
        }
    }

    private static void AddLogisticsRecommendations(
        ICollection<RecommendationDto> recommendations,
        RationProject ration,
        IReadOnlyList<DayAnalytics> dayAnalytics)
    {
        if (ration.Profile.Logistics.WeightImportance is WeightImportance.High or WeightImportance.Critical)
        {
            recommendations.Add(new RecommendationDto
            {
                Priority = 12,
                Message = "Профиль чувствителен к весу: контролируйте дневной вес и стремитесь к высокой калорийности на грамм."
            });

            if (dayAnalytics.Any(day => day.Nutrition.Weight > 900m))
            {
                recommendations.Add(new RecommendationDto
                {
                    Priority = 14,
                    Message = "В рационе есть тяжёлые дни. Для данного профиля стоит заменить часть массы на более энергоёмкие позиции."
                });
            }
        }

        if (ration.Profile.Logistics.CookingPossibility is CookingPossibility.Minimal or CookingPossibility.None)
        {
            recommendations.Add(new RecommendationDto
            {
                Priority = 18,
                Message = "При ограниченной готовке стоит делать упор на готовые к употреблению блюда и быстрые перекусы."
            });
        }

        if (ration.Profile.Logistics.ResupplyFrequency is ResupplyFrequency.Rare or ResupplyFrequency.None)
        {
            recommendations.Add(new RecommendationDto
            {
                Priority = 22,
                Message = "Редкое пополнение запасов требует более предсказуемого рациона и контроля плотности калорий."
            });
        }
    }

    private static void AddContentRecommendations(
        ICollection<RecommendationDto> recommendations,
        RationProject ration,
        IReadOnlyList<DayAnalytics> dayAnalytics)
    {
        var hasSnacks = ration.Days.Any(day =>
            day.Meals.Any(meal => meal.Type == MealType.Snack && meal.Items.Count > 0));

        if (ration.Profile.ActivityType is ActivityType.Speleo or ActivityType.Competition && !hasSnacks)
        {
            recommendations.Add(new RecommendationDto
            {
                Priority = 11,
                Message = "В профиле важны перекусы, но в рационе пока нет заполненных snack-приёмов пищи."
            });
        }

        if (ration.Profile.ActivityType == ActivityType.Speleo)
        {
            var onlyCookedMeals = ration.Days.All(day =>
                day.Meals.Where(meal => meal.Items.Count > 0).All(meal => meal.Type is MealType.Breakfast or MealType.Lunch or MealType.Dinner));

            if (onlyCookedMeals)
            {
                recommendations.Add(new RecommendationDto
                {
                    Priority = 13,
                    Message = "Спелео-рацион сейчас завязан на полноценные приёмы пищи. Добавьте варианты для питания на ходу."
                });
            }
        }

        if (ration.Profile.ActivityType == ActivityType.Competition &&
            dayAnalytics.Any(day => day.Nutrition.CaloriesPerGram < 3m))
        {
            recommendations.Add(new RecommendationDto
            {
                Priority = 16,
                Message = "Для соревнований часть дней выглядит слишком тяжёлой по массе. Попробуйте повысить калорийность на грамм."
            });
        }
    }

    private sealed record DayAnalytics(RationDay Day, Domain.ValueObjects.NutritionInfo Nutrition);
}
