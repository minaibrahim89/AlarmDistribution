namespace AlarmDistribution.WebApi.Application.Exceptions;

public class AlarmNotAssignedToNurseException(int alarmId, int nurseId) 
    : Exception($"Alarm with ID {alarmId} is not assigned to nurse with ID {nurseId}")
{
    public int AlarmId { get; } = alarmId;
    public int NurseId { get; } = nurseId;
}
