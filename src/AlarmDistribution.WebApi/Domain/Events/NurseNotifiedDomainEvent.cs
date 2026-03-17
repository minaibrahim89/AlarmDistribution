using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;
using Mediator;

namespace AlarmDistribution.WebApi.Domain.Events;

public class NurseNotifiedDomainEvent : DomainEventBase, INotification
{
    public NurseNotifiedDomainEvent(int nurseId, int alarmId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nurseId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alarmId);

        NurseId = nurseId;
        AlarmId = alarmId;
    }

    public int NurseId { get; }
    public int AlarmId { get; }
}
