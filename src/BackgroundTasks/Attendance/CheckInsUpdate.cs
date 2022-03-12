using System;
using KidsTown.BackgroundTasks.Common;
using KidsTown.PlanningCenterApiClient.Models.CheckInsResult;

namespace KidsTown.BackgroundTasks.Attendance;

public class CheckInsUpdate
{
    public readonly long CheckInsId;
    public readonly long? PeopleId;
    public readonly AttendeeType AttendeeType;
    public readonly string SecurityCode;
    public readonly int LocationId;
    public readonly DateTime CreationDate;
    public readonly PeopleUpdate Kid;
    public readonly string? EmergencyContactName;
    public readonly string? EmergencyContactNumber;

    public CheckInsUpdate(
        long checkInsId, 
        long? peopleId, 
        AttendeeType attendeeType,
        string securityCode,
        int locationId, 
        DateTime creationDate,
        PeopleUpdate kid, 
        string? emergencyContactName, 
        string? emergencyContactNumber)
    {
        CheckInsId = checkInsId;
        PeopleId = peopleId;
        SecurityCode = securityCode;

        LocationId = locationId;
        CreationDate = creationDate;
        Kid = kid;
        EmergencyContactName = emergencyContactName;
        EmergencyContactNumber = emergencyContactNumber;
        AttendeeType = attendeeType;
    }
}