using AlarmDistribution.WebApi.Domain.Events;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Domain.Aggregates.Nurses;

public class Nurse : EntityBase<int>
{
    // For EF Core
    public Nurse()
    {
    }

    public Nurse(int id, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
    }

    public string Name { get; }

    public IReadOnlyList<int> PendingAlarms => _pendingAlarms;

    private readonly List<int> _pendingAlarms = [];

    public void NotifyAlarm(int alarmId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alarmId);

        _pendingAlarms.Add(alarmId);

        RegisterDomainEvent(new NurseNotifiedDomainEvent(Id, alarmId));
    }

    public void RemovePendingAlarm(int alarmId)
    {
        _pendingAlarms.Remove(alarmId);
    }
}
