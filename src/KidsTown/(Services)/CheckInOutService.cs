using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public class CheckInOutService : ICheckInOutService
    {
        private readonly ICheckInOutRepository _checkInOutRepository;

        public CheckInOutService(ICheckInOutRepository checkInOutRepository)
        {
            _checkInOutRepository = checkInOutRepository;
        }
        
        public async Task<IImmutableList<Person>> SearchForPeople(PeopleSearchParameters searchParameters)
        {
            return await _checkInOutRepository.GetPeople(peopleSearchParameters: searchParameters).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<bool> CheckInOutPeople(CheckType checkType, IImmutableList<int> attendanceIds)
        {
            return checkType switch
            {
                CheckType.CheckIn => await CheckInPeople(attendanceIds: attendanceIds).ConfigureAwait(continueOnCapturedContext: false),
                CheckType.CheckOut => await CheckOutPeople(attendanceIds: attendanceIds).ConfigureAwait(continueOnCapturedContext: false),
                _ => throw new ArgumentException(message: $"CheckType unknown: {checkType}", paramName: nameof(checkType))
            };   
        }

        public Task<bool> UndoAction(CheckState revertedCheckState, ImmutableList<int> attendanceIds)
        {
            return _checkInOutRepository.SetCheckState(revertedCheckState: revertedCheckState, attendanceIds: attendanceIds);
        }

        public async Task<int?> CreateGuest(int locationId, string securityCode, string firstName, string lastName)
        {
            var securityCodeExists = await _checkInOutRepository.SecurityCodeExists(securityCode: securityCode);

            if (securityCodeExists)
            {
                return null;
            }
            
            return await _checkInOutRepository.CreateGuest(
                locationId: locationId,
                securityCode: securityCode,
                firstName: firstName,
                lastName: lastName);
        }
        
        private async Task<bool> CheckInPeople(IImmutableList<int> attendanceIds)
        {
            return await _checkInOutRepository.CheckInPeople(attendanceIds: attendanceIds).ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task<bool> CheckOutPeople(IImmutableList<int> attendanceIds)
        {
            return await _checkInOutRepository.CheckOutPeople(attendanceIds: attendanceIds).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}