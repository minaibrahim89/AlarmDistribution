using AlarmDistribution.WebApi.Application.Models;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AlarmDistribution.WebApi.Tests.Application.Models;

public class AlarmMonitorTests
{
    private readonly ISystemClock _systemClock;
    private readonly Alarm _testAlarm;
    private readonly Func<AlarmMonitor, Task> _noOpCallback;
    private readonly ILogger<AlarmMonitor> _loggerMock;

    public AlarmMonitorTests()
    {
        _systemClock = Substitute.For<ISystemClock>();
        _testAlarm = CreateValidAlarm();
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
    public async Task Constructor_DisposedIsInitiallyFalse()
    {
        // Arrange - Act
        var sut = new AlarmMonitor(_testAlarm, _noOpCallback, _loggerMock);

        // Assert
        Assert.False(sut.Disposed);
    }

    [Fact]
    public async Task Constructor_WhenRemainingTimeoutIsNotElapsed_DoesNotEscalate()
    {
        // Arrange
        var timeout = TimeSpan.FromMinutes(1);
        var currentClockTime = _testAlarm.Timestamp + timeout - TimeSpan.FromSeconds(30);
        _systemClock.UtcNow.Returns(currentClockTime);
        
        var isEscalated = false;
        var callback = (AlarmMonitor mon) =>
        {
            isEscalated = true;
            return Task.CompletedTask;
        };

        // Act
        _ = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _systemClock);

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
        Assert.False(isEscalated);
    }

    [Fact]
    public async Task Constructor_WhenEscalationTimeoutElapsedButAlarmWasAlreadyAcked_DoesNotEscalateAndDisposes()
    {
        // Arrange
        _testAlarm.Acknowledge(2);
        var timeout = TimeSpan.FromMinutes(1);
        var currentClockTime = _testAlarm.Timestamp + timeout;
        _systemClock.UtcNow.Returns(currentClockTime);

        var isEscalated = false;
        var callback = (AlarmMonitor mon) =>
        {
            isEscalated = true;
            return Task.CompletedTask;
        };

        // Act
        var sut = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _systemClock);

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
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
        _systemClock.UtcNow.Returns(_testAlarm.Timestamp + timeout);

        // Act
        var sut = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _systemClock);

        // Assert
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.True(isEscalated);
        Assert.True(sut.Disposed);
    }

    [Fact]
    public async Task Constructor_WhenRemainingTimeoutElapsedMultipleTimes_EscalatesOnlyOnce()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(1);
        var escalations = 0;
        var callback = (AlarmMonitor mon) =>
        {
            escalations++;
            return Task.CompletedTask;
        };
        _systemClock.UtcNow.Returns(_testAlarm.Timestamp + timeout);

        // Act
        _ = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _systemClock);
        _systemClock.UtcNow.Returns(_testAlarm.Timestamp + (2 * timeout));

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(3));
        Assert.Equal(1, escalations);
    }

    [Fact]
    public async Task Constructor_WhenErrorDuringEscalation_ErrorIsLogged()
    {
        // Arrange
        var timeout = TimeSpan.FromMinutes(1);
        var exception = new InvalidOperationException("Error occurred during escalation");
        Func<AlarmMonitor, Task> callback = (AlarmMonitor mon) =>
        {
            throw exception;
        };
        _systemClock.UtcNow.Returns(_testAlarm.Timestamp + timeout);

        // Act
        _ = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _systemClock);

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(1));
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
    public async Task Dispose_WhenEscalationTimeoutElapsed_DoesNotEscalate()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(1);
        var isEscalated = false;
        var callback = (AlarmMonitor mon) =>
        {
            isEscalated = true;
            return Task.CompletedTask;
        };
        _systemClock.UtcNow.Returns(_testAlarm.Timestamp);

        // Act
        var sut = new AlarmMonitor(_testAlarm, callback, _loggerMock, timeout, _systemClock);
        sut.Dispose();
        _systemClock.UtcNow.Returns(_testAlarm.Timestamp + timeout);

        // Assert
        await Task.Delay(TimeSpan.FromSeconds(3));
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
