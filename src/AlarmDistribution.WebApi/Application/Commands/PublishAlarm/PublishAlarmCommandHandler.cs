using AlarmDistribution.WebApi.Application.Services;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using Mediator;

namespace AlarmDistribution.WebApi.Application.Commands.PublishAlarm;

public class PublishAlarmCommandHandler : IRequestHandler<PublishAlarmCommand>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly IAlarmNotificationService _alarmNotificationService;

    public PublishAlarmCommandHandler(
        IAlarmRepository alarmRepository,
        IAlarmNotificationService alarmNotificationService)
    {
        ArgumentNullException.ThrowIfNull(alarmRepository);
        ArgumentNullException.ThrowIfNull(alarmNotificationService);

        _alarmRepository = alarmRepository;
        _alarmNotificationService = alarmNotificationService;
    }

    public async ValueTask<Unit> Handle(PublishAlarmCommand request, CancellationToken cancellationToken)
    {
        var alarm = new Alarm(request.AlarmId, request.PatientId, request.AlarmType, request.Timestamp);
            
        await _alarmRepository.AddAsync(alarm, cancellationToken);
        await _alarmNotificationService.NotifyPrimaryNurseAndMonitorAlarmForEscalationAsync(alarm, cancellationToken);
        return Unit.Value;
    }
}