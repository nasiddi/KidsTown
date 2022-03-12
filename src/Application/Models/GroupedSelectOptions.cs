// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Collections.Immutable;

namespace KidsTown.Application.Models;

public class GroupedSelectOptions
{
    public int GroupId { get; init; }
    public IImmutableList<SelectOption> Options { get; init; } = ImmutableList<SelectOption>.Empty;
    public int OptionCount { get; init; }
}