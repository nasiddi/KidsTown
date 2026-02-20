using System.Collections.Immutable;

namespace BackgroundTasks.Adult;

public record Family(int FamilyId, long HouseholdId, IImmutableList<Person> Members);