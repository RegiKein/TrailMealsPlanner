using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Domain.Entities;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class SaveDayAsTemplateHandler
{
    private readonly IRationProjectRepository rationRepository;
    private readonly IDayTemplateRepository templateRepository;

    public SaveDayAsTemplateHandler(IRationProjectRepository rationRepository, IDayTemplateRepository templateRepository)
    {
        this.rationRepository = rationRepository;
        this.templateRepository = templateRepository;
    }

    public async Task<Guid> Handle(SaveDayAsTemplateCommand command, CancellationToken cancellationToken = default)
    {
        if (command.RationId == Guid.Empty)
        {
            throw new ArgumentException("Ration id is required.", nameof(command));
        }

        if (command.DayId == Guid.Empty)
        {
            throw new ArgumentException("Day id is required.", nameof(command));
        }

        var ration = await rationRepository.GetByIdAsync(command.RationId, cancellationToken);
        if (ration is null)
        {
            throw new InvalidOperationException($"Ration '{command.RationId}' was not found.");
        }

        var day = ration.Days.FirstOrDefault(item => item.Id == command.DayId);
        if (day is null)
        {
            throw new InvalidOperationException($"Day '{command.DayId}' was not found in ration '{command.RationId}'.");
        }

        var template = DayTemplate.CreateTemplateFromDay(day, command.Name);
        await templateRepository.AddAsync(template, cancellationToken);
        return template.Id;
    }
}
