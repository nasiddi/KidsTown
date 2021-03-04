using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using CheckInsExtension.CheckInUpdateJobs.Update;
using CheckInsExtension.PlanningCenterAPIClient.Models.CheckInResult;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ConvertToUsingDeclaration

namespace ChekInsExtension.Database
{
    public class UpdateRepository : IUpdateRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IImmutableList<long>> GetExistingCheckInIds(IImmutableList<long> checkinIds)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var existingCheckInIds = await db.Attendances.Where(i => checkinIds.Contains(i.CheckInId))
                    .Select(i => i.CheckInId)
                    .ToListAsync();

                return existingCheckInIds.ToImmutableList();
            }
        }

        public async Task<ImmutableList<long>> GetPeopleIdsPreCheckedIns(int daysLookBack)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var peopleIds = await db.Attendances
                    .Where(i => i.InsertDate >= DateTime.Today.AddDays(-daysLookBack) && i.Person.PeopleId.HasValue)
                    .Select(i => i.Person.PeopleId!.Value)
                    .Distinct()
                    .ToListAsync();

                return peopleIds.ToImmutableList();
            }
        }

        public async Task UpdatePersons(ImmutableList<PeopleUpdate> peoples)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var existingPersons =
                    await GetPersonsByPeopleIds(db, peoples.Select(p => p.PeopleId!.Value).ToImmutableList());

                await UpdatePersons(db, existingPersons, peoples);
            }
        }

        public async Task InsertPreCheckIns(IImmutableList<CheckInUpdate> preCheckIns)
        {
            await using (var db = _serviceScopeFactory.CreateScope().ServiceProvider
                .GetRequiredService<CheckInsExtensionContext>())
            {
                var guests = preCheckIns.Where(c => c.PeopleId == null).ToImmutableList();

                await PreCheckInGuests(guests, db);

                var persons = preCheckIns
                    .Except(guests)
                    .Select(c => c.Person)
                    .GroupBy(p => p.PeopleId).Select(p => p.First())
                    .ToImmutableList();

                var existingPersons = await GetPersonsByPeopleIds(
                    db,
                    persons.Where(p => p.PeopleId.HasValue)
                        .Select(p => p.PeopleId!.Value).ToImmutableList());

                var personUpdates = persons.Where(p => existingPersons.Select(e => e.PeopleId).Contains(p.PeopleId))
                    .ToImmutableList();
                var personInserts = persons.Except(personUpdates).ToImmutableList();

                await UpdatePersons(db, existingPersons, personUpdates);

                var insertedPersons = await InsertPersons(db, personInserts);

                var checkIns = preCheckIns
                    .Except(guests)
                    .Select(c => MapToAttendance(c, existingPersons.Union(insertedPersons).ToImmutableList()))
                    .ToImmutableList();
                await db.AddRangeAsync(checkIns);
                await db.SaveChangesAsync();
            }
        }

        private async Task PreCheckInGuests(ImmutableList<CheckInUpdate> guests, CheckInsExtensionContext db)
        {
            var existingCheckInIds = await GetExistingCheckInIds(guests.Select(g => g.CheckInId).ToImmutableList());
            var newGuests = guests.Where(g => !existingCheckInIds.Contains(g.CheckInId)).ToImmutableList();

            var guestAttendances = newGuests.Select(MapGuestAttendance).ToImmutableList();
            
            await db.AddRangeAsync(guestAttendances);
            await db.SaveChangesAsync();
        }

        private static Attendance MapGuestAttendance(CheckInUpdate guest)
        {
            var person = MapPerson(guest.Person);
            return MapToAttendance(guest, ImmutableList.Create(person));
        }

        private static async Task<List<Person>> InsertPersons(CheckInsExtensionContext db,
            IImmutableList<PeopleUpdate> personToInsert)
        {
            var persons = personToInsert.Select(MapPerson).ToImmutableList();
            await db.AddRangeAsync(persons);
            await db.SaveChangesAsync();

            var insertedPeople = await db.People.Where(p => personToInsert.Select(i => i.PeopleId).Contains(p.PeopleId))
                .ToListAsync();

            return insertedPeople;
        }

        private static Person MapPerson(PeopleUpdate peopleUpdate)
        {
            return new()
            {
                PeopleId = peopleUpdate.PeopleId,
                FistName = peopleUpdate.FirstName,
                LastName = peopleUpdate.LastName,
                MayLeaveAlone = peopleUpdate.MayLeaveAlone,
                HasPeopleWithoutPickupPermission = peopleUpdate.HasPeopleWithoutPickupPermission
            };
        }

        private static async Task UpdatePersons(
            CheckInsExtensionContext db,
            List<Person> people,
            ImmutableList<PeopleUpdate> updates)
        {
            var updatesByPeopleId = updates.Where(u => u.PeopleId.HasValue)
                .ToImmutableDictionary(k => k.PeopleId!, v => v);

            people.ForEach(p =>
            {
                var update = updatesByPeopleId[p.PeopleId!];

                p.FistName = update.FirstName;
                p.LastName = update.LastName;
                p.MayLeaveAlone = update.MayLeaveAlone;
                p.HasPeopleWithoutPickupPermission = update.HasPeopleWithoutPickupPermission;
            });

            await db.SaveChangesAsync();
        }

        private static async Task<List<Person>> GetPersonsByPeopleIds(
            CheckInsExtensionContext db,
            IImmutableList<long> peopleIds)
        {
            var people = await db.People.Where(p => p.PeopleId.HasValue && peopleIds.Contains(p.PeopleId.Value))
                .ToListAsync();
            return people;
        }

        private static Attendance MapToAttendance(CheckInUpdate checkInUpdate, ImmutableList<Person> persons)
        {
            var person = persons.SingleOrDefault(p => p.PeopleId == checkInUpdate.PeopleId);

            return new Attendance
            {
                CheckInId = checkInUpdate.CheckInId,
                EventId = checkInUpdate.EventId,
                LocationId = (int) MapLocationId(checkInUpdate.Location),
                SecurityCode = checkInUpdate.SecurityCode,
                InsertDate = checkInUpdate.CreationDate,
                Person = person,
                AttendanceTypeId = MapAttendeeType(checkInUpdate.AttendeeType)
            };
        }

        private static Locations MapLocationId(string location)
        {
            return location switch
            {
                "Häsli" => Locations.Haesli,
                "Schöfli" => Locations.Schoefli,
                "Füchsli" => Locations.Fuechsli,
                "KidsChurch" => Locations.KidsChurch,
                "Kidschurch 1. Klasse" => Locations.KidsChurch,
                "Kidschurch 2. Klasse" => Locations.KidsChurch,
                "Kidschurch 3. Klasse" => Locations.KidsChurch,
                "Kidschurch 4. Klasse" => Locations.KidsChurch,
                "Kidschurch 5. Klasse" => Locations.KidsChurch,
                _ => throw new ArgumentOutOfRangeException(nameof(location), location, "Unknown location")
            };
        }

        private static int MapAttendeeType(AttendeeType attendeeType)
        {
            return attendeeType switch
            {
                AttendeeType.Regular => 1,
                AttendeeType.Guest => 2,
                AttendeeType.Volunteer => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(attendeeType), attendeeType, null)
            };
        }
    }
}