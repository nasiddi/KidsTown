using System.Collections.Immutable;

namespace KidsTown.KidsTown.Models;

public record PeopleSearchParameters(
    string SecurityCode,
    long EventId,
    IImmutableList<int> LocationGroups,
    bool UseFilterLocationGroups);