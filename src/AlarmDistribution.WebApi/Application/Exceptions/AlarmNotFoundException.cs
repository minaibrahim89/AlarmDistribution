namespace AlarmDistribution.WebApi.Application.Exceptions;

public class AlarmNotFoundException : Exception
{
    public AlarmNotFoundException(Guid alarmId) 
        : base($"Alarm with id {alarmId} is not found")
    {
        AlarmId = alarmId;
    }

    public Guid AlarmId { get; }
}
