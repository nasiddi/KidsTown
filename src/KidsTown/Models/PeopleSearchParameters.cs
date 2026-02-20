using System.Collections.Immutable;

namespace KidsTown.Models;

public record PeopleSearchParameters(
    string SecurityCode,
    long EventId,
    IImmutableList<int> LocationGroups,
    bool UseFilterLocationGroups);