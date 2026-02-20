using System.Collections.Immutable;

namespace BackgroundTasks.Adult;

public interface IAdultUpdateRepository
{
    Task<IImmutableList<Family>> GetFamiliesToUpdate(int daysLookBack, int take);
    Task<int> UpdateAdults(IImmutableList<AdultUpdate> parentUpdates);
    Task<int> RemovePeopleFromFamilies(ImmutableList<long> peopleIds);
    Task<int> SetFamilyUpdateDate(IImmutableList<Family> families);
    Task<ImmutableList<long>> GetVolunteerPersonIdsWithoutFamiliesToUpdate(int daysLookBack, int take);
    Task<int> UpdateVolunteers(ImmutableList<long> peopleIds, ImmutableList<VolunteerUpdate> volunteerUpdates);
}

public record AdultUpdate(long PeopleId, int FamilyId, long? PhoneNumberId, string FirstName, string LastName, string? PhoneNumber);

public record Person(long PeopleId, bool? IsChild);

public record VolunteerUpdate(long PeopleId, string FirstName, string LastName);