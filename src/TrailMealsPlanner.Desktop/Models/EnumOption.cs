using System;

namespace TrailMealsPlanner.Desktop.Models;

public sealed class EnumOption<T> where T : struct, Enum
{
    public EnumOption(T value, string display)
    {
        Value = value;
        Display = display;
    }

    public T Value { get; }

    public string Display { get; }
}
