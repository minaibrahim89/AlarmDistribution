using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Channels;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

namespace AlarmDistribution.WebApi.Application.Services;

public class MonitoredAlarmsService : IMonitoredAlarmsService
{
    private readonly ConcurrentDictionary<int, AlarmMonitor> _monitoredAlarms = [];

    private readonly ILogger<MonitoredAlarmsService> _logger;
    private readonly ILogger<AlarmMonitor> _alarmMonitorLogger;
    private readonly Channel<Alarm> _escalatedAlarmsChannel;

    public MonitoredAlarmsService(
        Channel<Alarm> escalatedAlarmsChannel,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(escalatedAlarmsChannel);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _escalatedAlarmsChannel = escalatedAlarmsChannel;
        _logger = loggerFactory.CreateLogger<MonitoredAlarmsService>();
        _alarmMonitorLogger = loggerFactory.CreateLogger<AlarmMonitor>();
    }

    public void StartAlarmMonitoring(Alarm alarm)
    {
        _monitoredAlarms.GetOrAdd(alarm.Id, _ =>
            new AlarmMonitor(alarm,
            OnAlarmEscalatedAsync, 
            _alarmMonitorLogger));
    }

    private async Task OnAlarmEscalatedAsync(AlarmMonitor monitor)
    {
        _logger.LogInformation("Escalating alarm with ID {AlarmId}", monitor.Alarm.Id);

        var alarm = monitor.Alarm;
        await _escalatedAlarmsChannel.Writer.WriteAsync(alarm);
        StopAlarmMonitoring(alarm.Id);
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