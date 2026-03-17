using AlarmDistribution.WebApi.Application.Services;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Events;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Application.EventHandler;

public class ClearAlarmWhenAlarmAckedDomainEventHandler : IDomainEventHandler<AlarmAckedDomainEvent>
{
    private readonly INurseRepository _nurseRepository;
    private readonly IMonitoredAlarmsService _monitoredAlarmsService;
    private readonly ILogger<ClearAlarmWhenAlarmAckedDomainEventHandler> _logger;

    public ClearAlarmWhenAlarmAckedDomainEventHandler(
        INurseRepository nurseRepository,
        IMonitoredAlarmsService monitoredAlarmsService,
        ILogger<ClearAlarmWhenAlarmAckedDomainEventHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(nurseRepository);
        ArgumentNullException.ThrowIfNull(monitoredAlarmsService);
        ArgumentNullException.ThrowIfNull(logger);

        _nurseRepository = nurseRepository;
        _monitoredAlarmsService = monitoredAlarmsService;
        _logger = logger;
    }

    public async ValueTask Handle(AlarmAckedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Alarm with ID {AlarmId} acknowledged, cancelling escalation if active", notification.AlarmId);
        
        _monitoredAlarmsService.StopAlarmMonitoring(notification.AlarmId);

        var nurse = await _nurseRepository.GetByIdAsync(notification.NurseId, true, cancellationToken);
        if (nurse != null)
        {
            nurse.RemovePendingAlarm(notification.AlarmId);
            await _nurseRepository.UpdateAsync(nurse, cancellationToken);
        }
    }
}
