// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Collections.Immutable;

namespace Application.Models
{
    public class GroupedSelectOptions
    {
        public int GroupId { get; init; }
        public ImmutableList<SelectOption> Options { get; init; } = ImmutableList<SelectOption>.Empty;
        public int OptionCount { get; init; }
    }
}