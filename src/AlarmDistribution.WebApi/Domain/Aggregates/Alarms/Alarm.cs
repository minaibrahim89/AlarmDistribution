using AlarmDistribution.WebApi.Domain.Events;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

public class Alarm : EntityBase<int>
{
    // For EF Core
    private Alarm()
    {
    }

    public Alarm(int alarmId, int patientId, AlarmType type, DateTimeOffset timestamp)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(alarmId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(patientId);

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid alarm type.");

        ArgumentOutOfRangeException.ThrowIfEqual(timestamp, default);

        Id = alarmId;
        PatientId = patientId;
        Type = type;
        Timestamp = timestamp;
    }

    public int PatientId { get; }

    public AlarmType Type { get; }

    public DateTimeOffset Timestamp { get; }

    public int? AcknowledgingNurseId { get; private set; }

    public DateTimeOffset? AcknowledgedAt { get; private set; }

    public bool IsAcknowledged => AcknowledgingNurseId is not null;

    public void Acknowledge(int nurseId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nurseId);

        if (IsAcknowledged)
            return;

        AcknowledgingNurseId = nurseId;
        AcknowledgedAt = DateTimeOffset.UtcNow;

        RegisterDomainEvent(new AlarmAckedDomainEvent(Id, PatientId, nurseId));
    }
}