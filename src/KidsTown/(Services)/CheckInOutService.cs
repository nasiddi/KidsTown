using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown.KidsTown;

public class CheckInOutService(
        ICheckInOutRepository checkInOutRepository,
        IConfigurationRepository configurationRepository,
        IPeopleRepository peopleRepository)
    : ICheckInOutService
{
    public async Task<IImmutableList<Kid>> SearchForPeople(PeopleSearchParameters searchParameters)
    {
        return await checkInOutRepository.GetPeople(searchParameters);
    }

    public async Task<bool> CheckInOutPeople(CheckType checkType, IImmutableList<int> attendanceIds)
    {
        return checkType switch
        {
            CheckType.CheckIn => await CheckInPeople(attendanceIds),
            CheckType.CheckOut => await CheckOutPeople(attendanceIds),
            CheckType.GuestCheckIn => throw new ArgumentException($"Unexpected CheckType: {checkType}", nameof(checkType)),
            _ => throw new ArgumentException($"CheckType unknown: {checkType}", nameof(checkType))
        };
    }

    public Task<bool> UndoAction(CheckState revertedCheckState, IImmutableList<int> attendanceIds)
    {
        return checkInOutRepository.SetCheckState(revertedCheckState, attendanceIds);
    }

    public async Task<int?> CreateGuest(int locationId, string securityCode, string firstName, string lastName)
    {
        var securityCodeExists = await checkInOutRepository.SecurityCodeExists(securityCode);

        if (securityCodeExists)
        {
            return null;
        }

        return await checkInOutRepository.CreateGuest(
                locationId,
                securityCode,
                firstName,
                lastName);
    }

    public async Task CreateUnregisteredGuest(
        string securityCode,
        long eventId,
        IImmutableList<int> selectedLocationGroupIds
    )
    {
        var locations = await configurationRepository.GetLocations(eventId);
        var location = TryMapUnregisteredGuestSecurityCodeToLocationId(securityCode, locations);
        if (location == null)
        {
            return;
        }

        if (!selectedLocationGroupIds.Contains(location.LocationGroupId))
        {
            return;
        }

        await peopleRepository.InsertUnregisteredGuest(securityCode, location.Id);
    }

    public async Task<bool> UpdateLocationAndCheckIn(int attendanceId, int locationId)
    {
        return await checkInOutRepository.UpdateLocationAndCheckIn(attendanceId, locationId);
    }

    private async Task<bool> CheckInPeople(IImmutableList<int> attendanceIds)
    {
        return await checkInOutRepository.CheckInPeople(attendanceIds);
    }

    private async Task<bool> CheckOutPeople(IImmutableList<int> attendanceIds)
    {
        return await checkInOutRepository.CheckOutPeople(attendanceIds);
    }

    private static Location? TryMapUnregisteredGuestSecurityCodeToLocationId(string securityCode, IImmutableList<Location> locations)
    {
        var locationString = securityCode.Substring(startIndex: 1, length: 1);
        var matchingLocations = locations.Where(l => l.Name.Contains(locationString)).ToList();
        return matchingLocations.Count == 1 ? matchingLocations.Single() : null;
    }
}