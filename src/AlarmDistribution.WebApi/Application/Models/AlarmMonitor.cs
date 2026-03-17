using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

namespace AlarmDistribution.WebApi.Application.Models;

public sealed class AlarmMonitor : IDisposable
{
    public static readonly TimeSpan DEFAULT_ESCALATION_TIMEOUT = TimeSpan.FromMinutes(1);

    private readonly Func<AlarmMonitor, Task> _onEscalate;
    private readonly Timer _escalationTimer;
    private readonly ILogger<AlarmMonitor> _logger;
        
    public AlarmMonitor(Alarm alarm, Func<AlarmMonitor, Task> onEscalate, ILogger<AlarmMonitor> logger, 
        TimeSpan? escalationTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(alarm);
        ArgumentNullException.ThrowIfNull(onEscalate);
        ArgumentNullException.ThrowIfNull(logger);

        Alarm = alarm;
        _onEscalate = onEscalate;
        _logger = logger;
        var timeout = escalationTimeout ?? DEFAULT_ESCALATION_TIMEOUT;

        var dueTime = alarm.Timestamp.UtcDateTime + timeout - DateTimeOffset.UtcNow;
        _escalationTimer = new Timer(EscalateAsync, alarm, dueTime, Timeout.InfiniteTimeSpan);
    }

    public Alarm Alarm { get; }
    public bool Disposed { get; private set; }

    private async void EscalateAsync(object? state)
    {
        try
        {
            if (!Alarm.IsAcknowledged)
                await _onEscalate(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating alarm with ID {AlarmId}", Alarm.Id);
        }
        finally
        {
            Dispose();
        }

    }

    public void Dispose()
    {
        if (Disposed) 
            return;

        _escalationTimer.Dispose();
        Disposed = true;

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation("Disposed alarm monitor for alarm with ID {AlarmId}", Alarm.Id);
    }
}
