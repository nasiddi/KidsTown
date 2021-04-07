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

        public async Task<bool> CheckInPeople(IImmutableList<int> checkInIds)
        {
            return await _checkInOutRepository.CheckInPeople(checkInIds: checkInIds).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<bool> CheckOutPeople(IImmutableList<int> checkInIds)
        {
            return await _checkInOutRepository.CheckOutPeople(checkInIds: checkInIds).ConfigureAwait(continueOnCapturedContext: false);
        }

        public Task<bool> UndoAction(CheckState revertedCheckState, ImmutableList<int> checkinIds)
        {
            return _checkInOutRepository.SetCheckState(revertedCheckState: revertedCheckState, checkInIds: checkinIds);
        }

        public async Task<int?> CheckInGuest(int locationId, string securityCode, string firstName, string lastName)
        {
            var attendanceId = await _checkInOutRepository.CreateGuest(
                locationId: locationId,
                securityCode: securityCode,
                firstName: firstName,
                lastName: lastName);
            
            var success = await CheckInPeople(checkInIds: ImmutableList.Create(item: attendanceId));

            return success ? attendanceId : null;
        }
    }
}