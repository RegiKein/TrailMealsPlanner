using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Domain.Entities;

public sealed class ProductPreference
{
    public ProductPreference(
        Guid participantId,
        Guid productId,
        PreferenceLevel preferenceLevel,
        string? comment = null)
    {
        if (participantId == Guid.Empty)
        {
            throw new ArgumentException("Participant id is required.", nameof(participantId));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        Id = Guid.NewGuid();
        ParticipantId = participantId;
        ProductId = productId;
        PreferenceLevel = preferenceLevel;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }

    public Guid Id { get; }

    public Guid ParticipantId { get; }

    public Guid ProductId { get; }

    public PreferenceLevel PreferenceLevel { get; private set; }

    public string? Comment { get; private set; }

    public void Update(PreferenceLevel preferenceLevel, string? comment)
    {
        PreferenceLevel = preferenceLevel;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }
}
