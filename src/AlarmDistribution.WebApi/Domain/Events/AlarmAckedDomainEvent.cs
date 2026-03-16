using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;
using Mediator;

namespace AlarmDistribution.WebApi.Domain.Events;

public class AlarmAckedDomainEvent : DomainEventBase, INotification
{
    public AlarmAckedDomainEvent(Guid alarmId, Guid nurseId)
    {
        ArgumentException.ThrowIfEmpty(alarmId);
        ArgumentException.ThrowIfEmpty(nurseId);

        AlarmId = alarmId;
        NurseId = nurseId;
    }

    public Guid AlarmId { get; }
    public Guid NurseId { get; }
}
