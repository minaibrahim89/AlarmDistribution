using Mediator;

namespace AlarmDistribution.WebApi.Application.Commands.AckAlarm;

public class AckAlarmCommand : IRequest
{
    public int AlarmId { get; private set; }

    public required int NurseId { get; set; }

    public void SetAlarmId(int alarmId)
    {
        AlarmId = alarmId;
    }
}
