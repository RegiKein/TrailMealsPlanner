namespace TrailMealsPlanner.Domain.Entities;

public sealed class Participant
{
    public Participant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Participant name is required.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public Guid Id { get; }

    public string Name { get; }
}
