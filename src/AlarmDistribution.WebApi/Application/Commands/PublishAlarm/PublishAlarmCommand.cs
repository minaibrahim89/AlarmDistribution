using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using Mediator;

namespace AlarmDistribution.WebApi.Application.Commands.PublishAlarm;

public class PublishAlarmCommand : IRequest
{
    public required Guid AlarmId { get; init; }

    public required Guid PatientId { get; init; }

    public required AlarmType AlarmType { get; init; }

    public required DateTimeOffset Timestamp { get; init; }
}
