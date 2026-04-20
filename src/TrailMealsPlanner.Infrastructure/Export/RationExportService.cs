using System.Globalization;
using System.Text;
using System.Text.Json;
using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Infrastructure.Export;

public sealed class RationExportService : IRationExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly IRationProjectRepository rationRepository;
    private readonly IDishRepository dishRepository;
    private readonly IProductRepository productRepository;

    public RationExportService(
        IRationProjectRepository rationRepository,
        IDishRepository dishRepository,
        IProductRepository productRepository)
    {
        this.rationRepository = rationRepository;
        this.dishRepository = dishRepository;
        this.productRepository = productRepository;
    }

    public async Task<string> ExportAsync(
        Guid rationId,
        RationExportFormat format,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        var ration = await rationRepository.GetByIdAsync(rationId, cancellationToken);
        if (ration is null)
        {
            throw new InvalidOperationException($"Ration '{rationId}' was not found.");
        }

        Directory.CreateDirectory(destinationDirectory);

        var dishes = await dishRepository.GetAllAsync(cancellationToken);
        var products = await productRepository.GetAllAsync(cancellationToken);
        var dishesById = dishes.ToDictionary(dish => dish.Id);
        var productsById = products.ToDictionary(product => product.Id);
        var safeName = MakeSafeFileName(ration.Name);
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);

        return format switch
        {
            RationExportFormat.Json => await ExportJson(ration, dishesById, productsById, destinationDirectory, safeName, timestamp, cancellationToken),
            RationExportFormat.Text => await ExportText(ration, dishesById, productsById, destinationDirectory, safeName, timestamp, cancellationToken),
            RationExportFormat.Pdf => await ExportPdf(ration, dishesById, productsById, destinationDirectory, safeName, timestamp, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format.")
        };
    }

    private static async Task<string> ExportJson(
        RationProject ration,
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById,
        string destinationDirectory,
        string safeName,
        string timestamp,
        CancellationToken cancellationToken)
    {
        var payload = BuildExportDocument(ration, dishesById, productsById);
        var path = Path.Combine(destinationDirectory, $"{safeName}-{timestamp}.json");
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        await File.WriteAllTextAsync(path, json, Encoding.UTF8, cancellationToken);
        return path;
    }

    private static async Task<string> ExportText(
        RationProject ration,
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById,
        string destinationDirectory,
        string safeName,
        string timestamp,
        CancellationToken cancellationToken)
    {
        var document = BuildExportDocument(ration, dishesById, productsById);
        var builder = new StringBuilder();

        builder.AppendLine($"Рацион: {document.Name}");
        builder.AppendLine($"Период: {document.StartDate:dd.MM.yyyy} - {document.EndDate:dd.MM.yyyy}");
        builder.AppendLine($"Участники: {document.ParticipantCount}");
        builder.AppendLine($"Калории: {document.Analytics.Calories:0.#} ккал");
        builder.AppendLine($"Вес: {document.Analytics.Weight:0.#} г");
        builder.AppendLine($"Плотность: {document.Analytics.CaloriesPerGram:0.##} ккал/г");
        builder.AppendLine();

        foreach (var day in document.Days)
        {
            builder.AppendLine($"День {day.DayNumber} - {day.Date:dd.MM.yyyy}");
            builder.AppendLine($"  Калории: {day.Analytics.Calories:0.#} ккал");
            builder.AppendLine($"  Вес: {day.Analytics.Weight:0.#} г");
            builder.AppendLine($"  Плотность: {day.Analytics.CaloriesPerGram:0.##} ккал/г");

            foreach (var meal in day.Meals)
            {
                builder.AppendLine($"  {meal.Type}");
                builder.AppendLine($"    Калории: {meal.Analytics.Calories:0.#} ккал");
                builder.AppendLine($"    Вес: {meal.Analytics.Weight:0.#} г");

                foreach (var item in meal.Items)
                {
                    builder.AppendLine($"    - {item.Name} x {item.Quantity:0.##}");
                }
            }

            builder.AppendLine();
        }

        var path = Path.Combine(destinationDirectory, $"{safeName}-{timestamp}.txt");
        await File.WriteAllTextAsync(path, builder.ToString(), Encoding.UTF8, cancellationToken);
        return path;
    }

    private static async Task<string> ExportPdf(
        RationProject ration,
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById,
        string destinationDirectory,
        string safeName,
        string timestamp,
        CancellationToken cancellationToken)
    {
        var textPath = await ExportText(ration, dishesById, productsById, destinationDirectory, safeName, timestamp, cancellationToken);
        var text = await File.ReadAllTextAsync(textPath, Encoding.UTF8, cancellationToken);
        var pdfPath = Path.Combine(destinationDirectory, $"{safeName}-{timestamp}.pdf");
        var pdfBytes = BuildSimplePdf(text);
        await File.WriteAllBytesAsync(pdfPath, pdfBytes, cancellationToken);
        return pdfPath;
    }

    private static ExportRationDocument BuildExportDocument(
        RationProject ration,
        IReadOnlyDictionary<Guid, Dish> dishesById,
        IReadOnlyDictionary<Guid, Product> productsById)
    {
        return new ExportRationDocument
        {
            Id = ration.Id,
            Name = ration.Name,
            StartDate = ration.StartDate,
            EndDate = ration.EndDate,
            ParticipantCount = ration.ParticipantCount,
            Analytics = ToExportNutrition(ration.CalculateNutrition(dishesById, productsById)),
            Days = ration.Days
                .OrderBy(day => day.DayNumber)
                .Select(day => new ExportRationDay
                {
                    DayNumber = day.DayNumber,
                    Date = day.Date,
                    Analytics = ToExportNutrition(day.CalculateNutrition(dishesById, productsById)),
                    Meals = day.Meals.Select(meal => new ExportMeal
                    {
                        Type = meal.Type.ToString(),
                        Analytics = ToExportNutrition(meal.CalculateNutrition(dishesById, productsById)),
                        Items = meal.Items.Select(item => new ExportMealItem
                        {
                            Name = item.DishId is Guid dishId && dishesById.TryGetValue(dishId, out var dish)
                                ? dish.Name
                                : item.ProductId is Guid productId && productsById.TryGetValue(productId, out var product)
                                    ? product.Name
                                    : "Unknown item",
                            Quantity = item.Quantity
                        }).ToList()
                    }).ToList()
                })
                .ToList()
        };
    }

    private static ExportNutrition ToExportNutrition(Domain.ValueObjects.NutritionInfo nutrition)
    {
        return new ExportNutrition
        {
            Calories = nutrition.Calories,
            Weight = nutrition.Weight,
            CaloriesPerGram = nutrition.CaloriesPerGram
        };
    }

    private static byte[] BuildSimplePdf(string text)
    {
        var sanitizedLines = text
            .Replace("\r", string.Empty)
            .Split('\n')
            .Select(EscapePdfText)
            .ToList();

        var content = new StringBuilder();
        content.AppendLine("BT");
        content.AppendLine("/F1 11 Tf");
        content.AppendLine("50 790 Td");
        content.AppendLine("14 TL");

        for (var index = 0; index < sanitizedLines.Count; index++)
        {
            if (index > 0)
            {
                content.AppendLine("T*");
            }

            content.AppendLine($"({sanitizedLines[index]}) Tj");
        }

        content.AppendLine("ET");

        var objects = new List<string>
        {
            "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj",
            "2 0 obj << /Type /Pages /Count 1 /Kids [3 0 R] >> endobj",
            "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >> endobj",
            "4 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj",
            $"5 0 obj << /Length {Encoding.ASCII.GetByteCount(content.ToString())} >> stream\n{content}endstream\nendobj"
        };

        var builder = new StringBuilder();
        builder.Append("%PDF-1.4\n");

        var offsets = new List<int> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(builder.ToString()));
            builder.Append(obj);
            builder.Append('\n');
        }

        var xrefPosition = Encoding.ASCII.GetByteCount(builder.ToString());
        builder.AppendLine("xref");
        builder.AppendLine($"0 {objects.Count + 1}");
        builder.AppendLine("0000000000 65535 f ");

        for (var i = 1; i < offsets.Count; i++)
        {
            builder.AppendLine($"{offsets[i]:D10} 00000 n ");
        }

        builder.AppendLine("trailer");
        builder.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        builder.AppendLine("startxref");
        builder.AppendLine(xrefPosition.ToString(CultureInfo.InvariantCulture));
        builder.Append("%%EOF");

        return Encoding.ASCII.GetBytes(builder.ToString());
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }

    private static string MakeSafeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var safeName = new string(value.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(safeName) ? "ration" : safeName;
    }

    private sealed class ExportRationDocument
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTime StartDate { get; init; }

        public DateTime EndDate { get; init; }

        public int ParticipantCount { get; init; }

        public ExportNutrition Analytics { get; init; } = new();

        public IReadOnlyList<ExportRationDay> Days { get; init; } = [];
    }

    private sealed class ExportRationDay
    {
        public int DayNumber { get; init; }

        public DateTime Date { get; init; }

        public ExportNutrition Analytics { get; init; } = new();

        public IReadOnlyList<ExportMeal> Meals { get; init; } = [];
    }

    private sealed class ExportMeal
    {
        public string Type { get; init; } = string.Empty;

        public ExportNutrition Analytics { get; init; } = new();

        public IReadOnlyList<ExportMealItem> Items { get; init; } = [];
    }

    private sealed class ExportMealItem
    {
        public string Name { get; init; } = string.Empty;

        public decimal Quantity { get; init; }
    }

    private sealed class ExportNutrition
    {
        public decimal Calories { get; init; }

        public decimal Weight { get; init; }

        public decimal CaloriesPerGram { get; init; }
    }
}
