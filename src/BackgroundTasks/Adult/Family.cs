using System.Collections.Immutable;

namespace KidsTown.BackgroundTasks.Adult;

public record Family(int FamilyId, long HouseholdId, IImmutableList<Person> Members);