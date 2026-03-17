namespace AlarmDistribution.WebApi.Application.Exceptions;

public class AlarmNotFoundException : Exception
{
    public AlarmNotFoundException(int alarmId) 
        : base($"Alarm with id {alarmId} is not found")
    {
        AlarmId = alarmId;
    }

    public int AlarmId { get; }
}
