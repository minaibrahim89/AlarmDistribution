using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

public class AlarmAcknowledgment : ValueObject
{
    public AlarmAcknowledgment(int nurseId, DateTimeOffset timestamp)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(nurseId);
        ArgumentException.ThrowIfEmpty(timestamp);

        AcknowledgingNurseId = nurseId;
        AcknowledgedAt = timestamp;
    }

    public int AcknowledgingNurseId { get; }

    public DateTimeOffset AcknowledgedAt { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AcknowledgingNurseId;
        yield return AcknowledgedAt;
    }
}
