using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;
using KidsTown.Shared;

namespace KidsTown.KidsTown;

public class CheckInOutService : ICheckInOutService
{
    private readonly ICheckInOutRepository _checkInOutRepository;
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IPeopleRepository _peopleRepository;

    public CheckInOutService(
        ICheckInOutRepository checkInOutRepository,
        IConfigurationRepository configurationRepository,
        IPeopleRepository peopleRepository
    )
    {
        _checkInOutRepository = checkInOutRepository;
        _configurationRepository = configurationRepository;
        _peopleRepository = peopleRepository;
    }
        
    public async Task<IImmutableList<Kid>> SearchForPeople(PeopleSearchParameters searchParameters)
    {
        return await _checkInOutRepository.GetPeople(peopleSearchParameters: searchParameters).ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task<bool> CheckInOutPeople(CheckType checkType, IImmutableList<int> attendanceIds)
    {
        return checkType switch
        {
            CheckType.CheckIn => await CheckInPeople(attendanceIds: attendanceIds).ConfigureAwait(continueOnCapturedContext: false),
            CheckType.CheckOut => await CheckOutPeople(attendanceIds: attendanceIds).ConfigureAwait(continueOnCapturedContext: false),
            CheckType.GuestCheckIn => throw new ArgumentException(message: $"Unexpected CheckType: {checkType}", paramName: nameof(checkType)),
            _ => throw new ArgumentException(message: $"CheckType unknown: {checkType}", paramName: nameof(checkType))
        };   
    }

    public Task<bool> UndoAction(CheckState revertedCheckState, IImmutableList<int> attendanceIds)
    {
        return _checkInOutRepository.SetCheckState(revertedCheckState: revertedCheckState, attendanceIds: attendanceIds);
    }

    public async Task<int?> CreateGuest(int locationId, string securityCode, string firstName, string lastName)
    {
        var securityCodeExists = await _checkInOutRepository.SecurityCodeExists(securityCode: securityCode)
            .ConfigureAwait(continueOnCapturedContext: false);

        if (securityCodeExists)
        {
            return null;
        }
            
        return await _checkInOutRepository.CreateGuest(
                locationId: locationId,
                securityCode: securityCode,
                firstName: firstName,
                lastName: lastName)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    public async Task CreateUnregisteredGuest(
        string securityCode,
        long eventId,
        IImmutableList<int> selectedLocationGroupIds
    )
    {
        var locations = await _configurationRepository.GetLocations(eventId: eventId);
        var location = TryMapUnregisteredGuestSecurityCodeToLocationId(securityCode: securityCode, locations: locations);
        if (location == null)
        {
            return;
        }

        if (!selectedLocationGroupIds.Contains(value: location.LocationGroupId))
        {
            return;
        }

        await _peopleRepository.InsertUnregisteredGuest(securityCode: securityCode, locationId: location.Id);
    }

    public async Task<bool> UpdateLocationAndCheckIn(int attendanceId, int locationId)
    {
        return await _checkInOutRepository.UpdateLocationAndCheckIn(attendanceId: attendanceId, locationId: locationId)
            .ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<bool> CheckInPeople(IImmutableList<int> attendanceIds)
    {
        return await _checkInOutRepository.CheckInPeople(attendanceIds: attendanceIds).ConfigureAwait(continueOnCapturedContext: false);
    }

    private async Task<bool> CheckOutPeople(IImmutableList<int> attendanceIds)
    {
        return await _checkInOutRepository.CheckOutPeople(attendanceIds: attendanceIds).ConfigureAwait(continueOnCapturedContext: false);
    }

    private static Location? TryMapUnregisteredGuestSecurityCodeToLocationId(string securityCode, IImmutableList<Location> locations)
    {
        var locationString = securityCode.Substring(startIndex: 1, length: 1);
        var matchingLocations = locations.Where(predicate: l => l.Name.Contains(value: locationString)).ToList();
        return matchingLocations.Count == 1 ? matchingLocations.Single() : null;
    }
}