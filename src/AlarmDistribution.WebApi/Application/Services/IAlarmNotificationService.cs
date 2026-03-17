using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

namespace AlarmDistribution.WebApi.Application.Services;

public interface IAlarmNotificationService
{
    public Task NotifyPrimaryNurseAndMonitorAlarmForEscalationAsync(Alarm alarm, CancellationToken cancellationToken);

    public void CancelEscalationMonitoring(int alarmId);
}
