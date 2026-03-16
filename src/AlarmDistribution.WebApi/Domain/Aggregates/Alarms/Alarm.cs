using AlarmDistribution.WebApi.Domain.Events;
using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

public class Alarm : EntityBase<Guid>
{
    // For EF Core
    private Alarm()
    {
    }

    public Alarm(Guid alarmId, Guid patientId, AlarmType type, DateTimeOffset timestamp)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(alarmId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(patientId, Guid.Empty);

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid alarm type.");

        ArgumentOutOfRangeException.ThrowIfEqual(timestamp, default);

        Id = alarmId;
        PatientId = patientId;
        Type = type;
        Timestamp = timestamp;
    }

    public Guid PatientId { get; }

    public AlarmType Type { get; }

    public DateTimeOffset Timestamp { get; }

    public Guid? AcknowledgingNurseId { get; private set; }

    public DateTimeOffset? AcknowledgedAt { get; private set; }

    public bool IsAcknowledged => AcknowledgingNurseId is not null;
    public void Acknowledge(Guid nurseId)
    {
        ArgumentException.ThrowIfEmpty(nurseId);

        if (IsAcknowledged)
            return;

        AcknowledgingNurseId = nurseId;
        AcknowledgedAt = DateTimeOffset.UtcNow;

        RegisterDomainEvent(new AlarmAckedDomainEvent(Id, nurseId));
    }
}