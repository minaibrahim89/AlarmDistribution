using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;
using Mediator;

namespace AlarmDistribution.WebApi.Domain.Events;

public class NurseNotifiedDomainEvent : DomainEventBase, INotification
{
    public NurseNotifiedDomainEvent(Guid nurseId, Guid alarmId)
    {
        ArgumentException.ThrowIfEmpty(nurseId);
        ArgumentException.ThrowIfEmpty(alarmId);

        NurseId = nurseId;
        AlarmId = alarmId;
    }

    public Guid NurseId { get; }
    public Guid AlarmId { get; }
}
