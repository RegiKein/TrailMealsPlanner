using System;
using TrailMealsPlanner.Application.DTO;

namespace TrailMealsPlanner.Desktop.ViewModels;

public sealed class WarningItemViewModel
{
    public WarningItemViewModel(WarningDto warning)
    {
        Code = warning.Code;
        Severity = warning.Severity;
        Scope = warning.Scope;
        Message = warning.Message;
        RelatedEntityId = warning.RelatedEntityId;
    }

    public string Code { get; }

    public WarningSeverity Severity { get; }

    public WarningScope Scope { get; }

    public string Message { get; }

    public Guid? RelatedEntityId { get; }

    public string SeverityLabel => Severity switch
    {
        WarningSeverity.Info => "Инфо",
        WarningSeverity.Warning => "Внимание",
        WarningSeverity.Critical => "Критично",
        _ => "Предупреждение"
    };

    public string Display => $"[{SeverityLabel}] {Message}";
}
