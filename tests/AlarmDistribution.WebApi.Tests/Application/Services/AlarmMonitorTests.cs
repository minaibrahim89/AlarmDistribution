using AlarmDistribution.WebApi.Application.Services;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace AlarmDistribution.WebApi.Tests.Application.Services;

public class AlarmMonitorTests
{
    private readonly FakeTimeProvider _timeProvider;
    private readonly Alarm _testAlarm;
    private readonly Func<AlarmMonitor, Task> _noOpCallback;
    private readonly ILogger<AlarmMonitor> _loggerMock;

    public AlarmMonitorTests()
    {
        _testAlarm = CreateValidAlarm();
        _timeProvider = new FakeTimeProvider(_testAlarm.Timestamp);
        _noOpCallback = (AlarmMonitor _) => Task.CompletedTask;
        _loggerMock = Substitute.For<ILogger<AlarmMonitor>>();
    }

    #region Constructor tests

    [Fact]
    public void Constructor_WhenAlarmIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        Alarm? nullAlarm = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AlarmMonitor(nullAlarm!, _noOpCallback, _loggerMock));

        Assert.Equal("alarm", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenOnEscalateCallbackIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AlarmMonitor(_testAlarm, null!, _loggerMock));

        Assert.Equal("onEscalate", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AlarmMonitor(_testAlarm, _noOpCallback, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_SetsCorrectEscalationTimeout(bool isEmpty)
    {
        // Arrange
        var timeout = isEmpty? (TimeSpan?)null : TimeSpan.FromSeconds(1);

        // Act
        var sut = new AlarmMonitor(_testAlarm, _noOpCallback, _loggerMock, timeout);

        // Assert
        Assert.Equal(isEmpty ? AlarmMonitor.DEFAULT_ESCALATION_TIMEOUT : timeout, sut.EscalationTimeout);
    }

    [Fact]
    public void Constructor_DisposedIsInitiallyFalse()
    {
        // Arrange - Act
        var sut = new AlarmMonitor(_testAlarm, _noOpCallback, _loggerMock);

        // Assert
        Assert.False(sut.Disposed);
    }

    [Fact]
    public void Constructor_WhenRemainingTimeoutIsNotElapsed_DoesNotEscalate()
    {
        // Arrange
        var timeout = TimeSpan.FromMinutes(1);
        var isEscalated = false;
        var callback = (AlarmMonitor mon) =>
        {
            isEscalated = true;
            return Task.CompletedTask;
        };

        // Act
        _ = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _timeProvider);
        _timeProvider.Advance(timeout - TimeSpan.FromSeconds(30));

        // Assert
        Assert.False(isEscalated);
    }

    [Fact]
    public void Constructor_WhenEscalationTimeoutElapsedButAlarmWasAlreadyAcked_DoesNotEscalateAndDisposes()
    {
        // Arrange
        _testAlarm.Acknowledge(2);
        var timeout = TimeSpan.FromMinutes(1);

        var isEscalated = false;
        var callback = (AlarmMonitor mon) =>
        {
            isEscalated = true;
            return Task.CompletedTask;
        };

        // Act
        var sut = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _timeProvider);
        _timeProvider.Advance(timeout);

        // Assert
        Assert.False(isEscalated);
        Assert.True(sut.Disposed);
    }

    [Fact]
    public async Task Constructor_WhenRemainingTimeoutElapsed_EscalatesAndDisposes()
    {
        // Arrange
        var timeout = TimeSpan.FromMinutes(1);
        var isEscalated = false;
        var taskCompletionSource = new TaskCompletionSource<bool>();
        var callback = (AlarmMonitor mon) =>
        {
            isEscalated = true;
            taskCompletionSource.SetResult(true);
            return Task.CompletedTask;
        };

        // Act
        var sut = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _timeProvider);
        _timeProvider.Advance(timeout);

        // Assert
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(isEscalated);
        Assert.True(SpinWait.SpinUntil(() => sut.Disposed, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Constructor_WhenRemainingTimeoutElapsedMultipleTimes_EscalatesOnlyOnce()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(1);
        var escalations = 0;
        var callback = (AlarmMonitor mon) =>
        {
            escalations++;
            return Task.CompletedTask;
        };

        // Act
        _ = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _timeProvider);
        _timeProvider.Advance(timeout);
        _timeProvider.Advance(timeout);

        // Assert
        Assert.Equal(1, escalations);
    }

    [Fact]
    public async Task Constructor_WhenEscalatonTimeoutHasAlreadyBeenElapsedAfterTimerArrived_EscalatesImmediately()
    {
        // Arrange
        var timeout = TimeSpan.FromMinutes(1);
        var isEscalated = false;
        var taskCompletionSource = new TaskCompletionSource<bool>();
        var callback = (AlarmMonitor mon) =>
        {
            isEscalated = true;
            taskCompletionSource.SetResult(true);
            return Task.CompletedTask;
        };
        _timeProvider.SetUtcNow(_testAlarm.Timestamp + (2 * timeout));

        // Act
        var sut = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _timeProvider);

        // Assert
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await Task.Delay(TimeSpan.FromSeconds(2));
        Assert.True(isEscalated);
        Assert.True(sut.Disposed);
    }

    [Fact]
    public void Constructor_WhenErrorDuringEscalation_ErrorIsLogged()
    {
        // Arrange
        var timeout = TimeSpan.FromMinutes(1);
        var exception = new InvalidOperationException("Error occurred during escalation");
        Func<AlarmMonitor, Task> callback = (AlarmMonitor mon) =>
        {
            throw exception;
        };

        // Act
        _ = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _timeProvider);
        _timeProvider.Advance(timeout);

        // Assert
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region Dispose tests

    [Fact]
    public void Dispose_WhenEscalationTimeoutElapsed_DoesNotEscalate()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(1);
        var isEscalated = false;
        var callback = (AlarmMonitor mon) =>
        {
            isEscalated = true;
            return Task.CompletedTask;
        };
        _timeProvider.SetUtcNow(_testAlarm.Timestamp);

        // Act
        var sut = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _timeProvider);
        sut.Dispose();
        _timeProvider.Advance(timeout);

        // Assert
        Assert.True(sut.Disposed);
        Assert.False(isEscalated);
    }

    #endregion

    #region Helpers

    private static Alarm CreateValidAlarm()
    {
        return new Alarm(
            alarmId: 1,
            patientId: 100,
            type: AlarmType.HeartRate,
            timestamp: DateTimeOffset.UtcNow);
    }

    #endregion
}
