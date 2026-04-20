using TrailMealsPlanner.Application.DTO;
using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;
using TrailMealsPlanner.Domain.ValueObjects;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class GetRationWarningsHandler
{
    private readonly IRationProjectRepository rationRepository;
    private readonly IDishRepository dishRepository;
    private readonly IProductRepository productRepository;

    public GetRationWarningsHandler(
        IRationProjectRepository rationRepository,
        IDishRepository dishRepository,
        IProductRepository productRepository)
    {
        this.rationRepository = rationRepository;
        this.dishRepository = dishRepository;
        this.productRepository = productRepository;
    }

    public async Task<RationWarningsDto?> Handle(
        GetRationWarningsQuery query,
        CancellationToken cancellationToken = default)
    {
        var ration = await rationRepository.GetByIdAsync(query.RationId, cancellationToken);
        if (ration is null)
        {
            return null;
        }

        var dishes = await dishRepository.GetAllAsync(cancellationToken);
        var products = await productRepository.GetAllAsync(cancellationToken);

        return RationWarningsEvaluator.Evaluate(
            ration,
            dishes.ToDictionary(dish => dish.Id),
            products.ToDictionary(product => product.Id));
    }

    private static class RationWarningsEvaluator
    {
        public static RationWarningsDto Evaluate(
            RationProject ration,
            IReadOnlyDictionary<Guid, Dish> dishesById,
            IReadOnlyDictionary<Guid, Product> productsById)
        {
            var thresholds = WarningThresholds.ForProfile(ration.Profile);
            var rationNutrition = ration.CalculateNutrition(dishesById, productsById);
            var rationWarnings = BuildRationWarnings(ration, rationNutrition, thresholds);
            var dayWarnings = ration.Days
                .OrderBy(day => day.DayNumber)
                .Select(day => BuildDayWarnings(day, dishesById, productsById, thresholds))
                .ToList();

            return new RationWarningsDto
            {
                RationId = ration.Id,
                RationWarnings = rationWarnings,
                Days = dayWarnings
            };
        }

        private static IReadOnlyList<WarningDto> BuildRationWarnings(
            RationProject ration,
            NutritionInfo rationNutrition,
            WarningThresholds thresholds)
        {
            var warnings = new List<WarningDto>();

            if (rationNutrition.Calories <= 0)
            {
                warnings.Add(new WarningDto
                {
                    Code = "ration.empty",
                    Severity = WarningSeverity.Critical,
                    Scope = WarningScope.Ration,
                    Message = "Рацион пока не содержит калорийных позиций. Заполните хотя бы один день.",
                    RelatedEntityId = ration.Id
                });

                return warnings;
            }

            var macroCalories = (rationNutrition.Protein * 4m) + (rationNutrition.Fat * 9m) + (rationNutrition.Carbs * 4m);
            if (macroCalories <= 0)
            {
                return warnings;
            }

            var proteinShare = rationNutrition.Protein * 4m / macroCalories;
            var fatShare = rationNutrition.Fat * 9m / macroCalories;
            var carbsShare = rationNutrition.Carbs * 4m / macroCalories;

            var deviations = new List<string>();
            var severity = WarningSeverity.Info;

            AppendMacroDeviation(deviations, "белки", proteinShare, thresholds.ProteinShareMin, thresholds.ProteinShareMax, ref severity);
            AppendMacroDeviation(deviations, "жиры", fatShare, thresholds.FatShareMin, thresholds.FatShareMax, ref severity);
            AppendMacroDeviation(deviations, "углеводы", carbsShare, thresholds.CarbsShareMin, thresholds.CarbsShareMax, ref severity);

            if (deviations.Count > 0)
            {
                warnings.Add(new WarningDto
                {
                    Code = "ration.macro-imbalance",
                    Severity = severity,
                    Scope = WarningScope.Ration,
                    Message = $"Рацион: БЖУ заметно отклоняются от рекомендуемого диапазона: {string.Join("; ", deviations)}.",
                    RelatedEntityId = ration.Id
                });
            }

            return warnings;
        }

        private static RationDayWarningsDto BuildDayWarnings(
            RationDay day,
            IReadOnlyDictionary<Guid, Dish> dishesById,
            IReadOnlyDictionary<Guid, Product> productsById,
            WarningThresholds thresholds)
        {
            var warnings = new List<WarningDto>();
            var nutrition = day.CalculateNutrition(dishesById, productsById);
            var totalItems = day.Meals.Sum(meal => meal.Items.Count);

            if (totalItems == 0)
            {
                warnings.Add(new WarningDto
                {
                    Code = "day.empty",
                    Severity = WarningSeverity.Critical,
                    Scope = WarningScope.Day,
                    Message = $"День {day.DayNumber}: день пока не заполнен.",
                    RelatedEntityId = day.Id
                });
            }
            else
            {
                foreach (var meal in day.Meals.Where(meal => meal.Items.Count == 0))
                {
                    warnings.Add(new WarningDto
                    {
                        Code = "meal.empty",
                        Severity = WarningSeverity.Info,
                        Scope = WarningScope.Meal,
                        Message = $"День {day.DayNumber}, {GetMealDisplayName(meal.Type)}: приём пищи пока не заполнен.",
                        RelatedEntityId = meal.Id
                    });
                }
            }

            if (nutrition.Calories < thresholds.TargetDayCalories)
            {
                var delta = thresholds.TargetDayCalories - nutrition.Calories;
                warnings.Add(new WarningDto
                {
                    Code = "day.low-calories",
                    Severity = delta >= thresholds.TargetDayCalories * 0.25m ? WarningSeverity.Critical : WarningSeverity.Warning,
                    Scope = WarningScope.Day,
                    Message = $"День {day.DayNumber}: калорийность ниже целевой на {Math.Round(delta, 0):0} ккал.",
                    RelatedEntityId = day.Id
                });
            }

            if (nutrition.Weight > thresholds.MaxDayWeight)
            {
                var delta = nutrition.Weight - thresholds.MaxDayWeight;
                warnings.Add(new WarningDto
                {
                    Code = "day.overweight",
                    Severity = delta >= 250m ? WarningSeverity.Critical : WarningSeverity.Warning,
                    Scope = WarningScope.Day,
                    Message = $"День {day.DayNumber}: вес выше допустимого на {Math.Round(delta, 0):0} г.",
                    RelatedEntityId = day.Id
                });
            }

            if (nutrition.Weight > 0 && nutrition.CaloriesPerGram < thresholds.MinCaloriesPerGram)
            {
                warnings.Add(new WarningDto
                {
                    Code = "day.low-density",
                    Severity = thresholds.MinCaloriesPerGram - nutrition.CaloriesPerGram >= 0.5m
                        ? WarningSeverity.Warning
                        : WarningSeverity.Info,
                    Scope = WarningScope.Day,
                    Message = $"День {day.DayNumber}: энергетическая плотность {nutrition.CaloriesPerGram:0.##} ккал/г ниже рекомендуемой {thresholds.MinCaloriesPerGram:0.##} ккал/г.",
                    RelatedEntityId = day.Id
                });
            }

            return new RationDayWarningsDto
            {
                DayId = day.Id,
                DayNumber = day.DayNumber,
                Warnings = warnings
                    .OrderByDescending(warning => warning.Severity)
                    .ThenBy(warning => warning.Scope)
                    .ToList()
            };
        }

        private static void AppendMacroDeviation(
            ICollection<string> deviations,
            string name,
            decimal actualShare,
            decimal minShare,
            decimal maxShare,
            ref WarningSeverity severity)
        {
            if (actualShare < minShare)
            {
                var gap = minShare - actualShare;
                deviations.Add($"{name} {actualShare:P0} при норме {minShare:P0}-{maxShare:P0}");
                severity = MaxSeverity(severity, gap >= 0.12m ? WarningSeverity.Critical : WarningSeverity.Warning);
            }
            else if (actualShare > maxShare)
            {
                var gap = actualShare - maxShare;
                deviations.Add($"{name} {actualShare:P0} при норме {minShare:P0}-{maxShare:P0}");
                severity = MaxSeverity(severity, gap >= 0.12m ? WarningSeverity.Critical : WarningSeverity.Warning);
            }
        }

        private static string GetMealDisplayName(MealType mealType)
        {
            return mealType switch
            {
                MealType.Breakfast => "завтрак",
                MealType.Lunch => "обед",
                MealType.Dinner => "ужин",
                MealType.Snack => "перекус",
                MealType.Emergency => "аварийный запас",
                _ => mealType.ToString()
            };
        }

        private static WarningSeverity MaxSeverity(WarningSeverity left, WarningSeverity right)
        {
            return left >= right ? left : right;
        }
    }

    private sealed record WarningThresholds(
        decimal TargetDayCalories,
        decimal MaxDayWeight,
        decimal MinCaloriesPerGram,
        decimal ProteinShareMin,
        decimal ProteinShareMax,
        decimal FatShareMin,
        decimal FatShareMax,
        decimal CarbsShareMin,
        decimal CarbsShareMax)
    {
        public static WarningThresholds ForProfile(RationProfile profile)
        {
            var targetDayCalories = profile.ActivityType switch
            {
                ActivityType.Alpine => 4000m,
                ActivityType.Ski => 3600m,
                ActivityType.Mountain => 3400m,
                ActivityType.Competition => 3200m,
                ActivityType.Cycling => 3200m,
                ActivityType.Speleo => 3000m,
                ActivityType.Hunting => 3000m,
                ActivityType.Hiking => 2800m,
                ActivityType.Horseback => 2800m,
                ActivityType.Sailing => 2600m,
                ActivityType.Water => 2500m,
                ActivityType.WeekendTrip => 2300m,
                ActivityType.AutoMoto => 2200m,
                ActivityType.LeisureWalk => 1800m,
                _ => 2600m
            };

            targetDayCalories += profile.Environment.TemperatureRange switch
            {
                TemperatureRange.Cold => 250m,
                TemperatureRange.ExtremeCold => 500m,
                TemperatureRange.Hot => 100m,
                _ => 0m
            };

            targetDayCalories += profile.Environment.AltitudeRange switch
            {
                AltitudeRange.High => 150m,
                AltitudeRange.Extreme => 300m,
                _ => 0m
            };

            var maxDayWeight = profile.Logistics.WeightImportance switch
            {
                WeightImportance.Critical => 700m,
                WeightImportance.High => 850m,
                WeightImportance.Medium => 1100m,
                _ => 1400m
            };

            var minCaloriesPerGram = profile.Logistics.WeightImportance switch
            {
                WeightImportance.Critical => 3.5m,
                WeightImportance.High => 3.0m,
                WeightImportance.Medium => 2.5m,
                _ => 2.0m
            };

            if (profile.ActivityType == ActivityType.Competition)
            {
                return new WarningThresholds(
                    targetDayCalories,
                    maxDayWeight,
                    minCaloriesPerGram,
                    0.10m,
                    0.20m,
                    0.18m,
                    0.30m,
                    0.55m,
                    0.72m);
            }

            if (profile.ActivityType == ActivityType.Alpine)
            {
                return new WarningThresholds(
                    targetDayCalories,
                    maxDayWeight,
                    minCaloriesPerGram,
                    0.10m,
                    0.20m,
                    0.25m,
                    0.42m,
                    0.38m,
                    0.58m);
            }

            return new WarningThresholds(
                targetDayCalories,
                maxDayWeight,
                minCaloriesPerGram,
                0.10m,
                0.22m,
                0.20m,
                0.38m,
                0.40m,
                0.65m);
        }
    }
}
