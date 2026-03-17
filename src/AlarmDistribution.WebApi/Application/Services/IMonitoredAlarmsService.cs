using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

namespace AlarmDistribution.WebApi.Application.Services;

public interface IMonitoredAlarmsService
{
    void StartAlarmMonitoring(Alarm alarm);

    void StopAlarmMonitoring(int alarmId);
}
