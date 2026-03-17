using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;
using Mediator;

namespace AlarmDistribution.WebApi.Domain.Events;

public class AlarmAckedDomainEvent : DomainEventBase, INotification
{
    public AlarmAckedDomainEvent(int alarmId, int nurseId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alarmId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nurseId);

        AlarmId = alarmId;
        NurseId = nurseId;
    }

    public int AlarmId { get; }
    public int NurseId { get; }
}
