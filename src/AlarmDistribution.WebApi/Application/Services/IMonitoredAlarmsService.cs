using AlarmDistribution.WebApi.Application.Model;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;

namespace AlarmDistribution.WebApi.Application.Services;

public interface IMonitoredAlarmsService
{
    void StartAlarmMonitoring(Alarm alarm, Func<AlarmMonitor, Task> callback);

    void StopAlarmMonitoring(int alarmId);
}
