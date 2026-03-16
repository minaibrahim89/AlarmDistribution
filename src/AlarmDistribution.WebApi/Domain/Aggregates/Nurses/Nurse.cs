using AlarmDistribution.WebApi.Domain.Events;
using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Domain.Aggregates.Nurses;

public class Nurse : EntityBase<Guid>
{
    // For EF Core
    public Nurse()
    {
    }

    public Nurse(Guid id, string name)
    {
        ArgumentException.ThrowIfEmpty(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<Guid> PendingAlarms => _pendingAlarms;

    private readonly List<Guid> _pendingAlarms = [];

    public void NotifyAlarm(Guid alarmId)
    {
        ArgumentException.ThrowIfEmpty(alarmId);

        _pendingAlarms.Add(alarmId);

        RegisterDomainEvent(new NurseNotifiedDomainEvent(Id, alarmId));
    }

    public void RemovePendingAlarm(Guid alarmId)
    {
        _pendingAlarms.Remove(alarmId);
    }
}
