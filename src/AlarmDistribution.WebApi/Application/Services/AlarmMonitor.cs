using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

namespace AlarmDistribution.WebApi.Application.Services;

public sealed class AlarmMonitor : IDisposable
{
    public static readonly TimeSpan DEFAULT_ESCALATION_TIMEOUT = TimeSpan.FromMinutes(1);

    private readonly Func<AlarmMonitor, Task> _onEscalate;
    private readonly ITimer _escalationTimer;
    private readonly ILogger<AlarmMonitor> _logger;

    public AlarmMonitor(Alarm alarm, Func<AlarmMonitor, Task> onEscalate, ILogger<AlarmMonitor> logger,
        TimeSpan? escalationTimeout = null, TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(alarm);
        ArgumentNullException.ThrowIfNull(onEscalate);
        ArgumentNullException.ThrowIfNull(logger);

        Alarm = alarm;
        _onEscalate = onEscalate;
        _logger = logger;
        EscalationTimeout = escalationTimeout ?? DEFAULT_ESCALATION_TIMEOUT;
        timeProvider ??= TimeProvider.System;

        var dueTime = alarm.Timestamp.UtcDateTime + EscalationTimeout - timeProvider.GetUtcNow();

        if (dueTime < TimeSpan.Zero)
            // It is already too late, escalate immediately
            dueTime = TimeSpan.Zero;

        _escalationTimer = timeProvider.CreateTimer(EscalateAsync, alarm, dueTime, Timeout.InfiniteTimeSpan);
    }

    public Alarm Alarm { get; }
    public TimeSpan EscalationTimeout { get; }
    public bool Disposed { get; private set; }

    private async void EscalateAsync(object? state)
    {
        try
        {
            if (Alarm.IsAcknowledged)
            {
                _logger.LogInformation("Alarm with ID {AlarmId} acknowledged before escalation, skipping escalation", Alarm.Id);
                return;
            }

            _logger.LogInformation("Escalating alarm with ID {AlarmId} after timeout of {EscalationTimeout}", Alarm.Id, EscalationTimeout);
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

        _logger.LogDebug("Disposed alarm monitor for alarm with ID {AlarmId}", Alarm.Id);
    }
}
