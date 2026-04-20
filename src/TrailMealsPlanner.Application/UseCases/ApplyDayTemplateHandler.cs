using TrailMealsPlanner.Application.Interfaces;

namespace TrailMealsPlanner.Application.UseCases;

public sealed class ApplyDayTemplateHandler
{
    private readonly IRationProjectRepository rationRepository;
    private readonly IDayTemplateRepository templateRepository;

    public ApplyDayTemplateHandler(IRationProjectRepository rationRepository, IDayTemplateRepository templateRepository)
    {
        this.rationRepository = rationRepository;
        this.templateRepository = templateRepository;
    }

    public async Task Handle(ApplyDayTemplateCommand command, CancellationToken cancellationToken = default)
    {
        if (command.RationId == Guid.Empty)
        {
            throw new ArgumentException("Ration id is required.", nameof(command));
        }

        if (command.TemplateId == Guid.Empty)
        {
            throw new ArgumentException("Template id is required.", nameof(command));
        }

        if (command.TargetDayId == Guid.Empty)
        {
            throw new ArgumentException("Target day id is required.", nameof(command));
        }

        var ration = await rationRepository.GetByIdAsync(command.RationId, cancellationToken);
        if (ration is null)
        {
            throw new InvalidOperationException($"Ration '{command.RationId}' was not found.");
        }

        var template = await templateRepository.GetByIdAsync(command.TemplateId, cancellationToken);
        if (template is null)
        {
            throw new InvalidOperationException($"Template '{command.TemplateId}' was not found.");
        }

        ration.ApplyDayTemplate(template, command.TargetDayId);
    }
}
