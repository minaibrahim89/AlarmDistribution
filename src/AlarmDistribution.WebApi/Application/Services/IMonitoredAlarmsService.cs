using AlarmDistribution.WebApi.Application.Models;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

namespace AlarmDistribution.WebApi.Application.Services;

public interface IMonitoredAlarmsService
{
    void StartAlarmMonitoring(Alarm alarm, Func<AlarmMonitor, Task> callback);

    void StopAlarmMonitoring(int alarmId);
}
