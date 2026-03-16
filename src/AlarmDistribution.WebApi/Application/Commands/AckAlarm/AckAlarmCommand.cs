using Mediator;

namespace AlarmDistribution.WebApi.Application.Commands.AckAlarm;

public class AckAlarmCommand : IRequest
{
    public Guid AlarmId { get; private set; }

    public required Guid NurseId { get; set; }

    public void SetAlarmId(Guid alarmId)
    {
        AlarmId = alarmId;
    }
}
