namespace AlarmDistribution.WebApi.Application.Exceptions;

public class AlarmNotAssignedToNurseException(Guid alarmId, Guid nurseId) 
    : Exception($"Alarm with ID {alarmId} is not assigned to nurse with ID {nurseId}")
{
    public Guid AlarmId { get; } = alarmId;
    public Guid NurseId { get; } = nurseId;
}
