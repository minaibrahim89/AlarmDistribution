using Mediator;

namespace AlarmDistribution.WebApi.Application.Commands.AckAlarm;

public class AckAlarmCommand : IRequest
{
    internal int AlarmId { get; set; }

    public required int NurseId { get; set; }
}
