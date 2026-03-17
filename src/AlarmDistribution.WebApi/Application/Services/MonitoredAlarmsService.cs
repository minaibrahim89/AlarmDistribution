using System.Collections.Concurrent;
using AlarmDistribution.WebApi.Application.Model;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

namespace AlarmDistribution.WebApi.Application.Services;

public class MonitoredAlarmsService : IMonitoredAlarmsService
{
    private readonly ConcurrentDictionary<int, AlarmMonitor> _monitoredAlarms = [];

    private readonly ILogger<MonitoredAlarmsService> _logger;
    private readonly ILogger<AlarmMonitor> _alarmMonitorLogger;

    public MonitoredAlarmsService(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _logger = loggerFactory.CreateLogger<MonitoredAlarmsService>();
        _alarmMonitorLogger = loggerFactory.CreateLogger<AlarmMonitor>();
    }

    public void StartAlarmMonitoring(Alarm alarm, Func<AlarmMonitor, Task> callback)
    {
        _monitoredAlarms.GetOrAdd(alarm.Id, _ =>
            new AlarmMonitor(alarm, callback, _alarmMonitorLogger));
    }

    public void StopAlarmMonitoring(int alarmId)
    {
        if (_monitoredAlarms.TryGetValue(alarmId, out var alarmMonitor))
            CleanUpMonitor(alarmMonitor);
    }

    private void CleanUpMonitor(AlarmMonitor alarmMonitor)
    {
        alarmMonitor.Dispose();
        _monitoredAlarms.TryRemove(alarmMonitor.Alarm.Id, out _);

        _logger.LogInformation("Stopped monitoring alarm with ID {AlarmId}", alarmMonitor.Alarm.Id);
    }
}