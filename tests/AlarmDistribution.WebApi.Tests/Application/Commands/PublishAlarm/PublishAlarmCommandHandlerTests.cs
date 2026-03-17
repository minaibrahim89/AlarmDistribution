using AlarmDistribution.WebApi.Application.Commands.PublishAlarm;
using AlarmDistribution.WebApi.Application.Exceptions;
using AlarmDistribution.WebApi.Application.Models;
using AlarmDistribution.WebApi.Application.Services;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AlarmDistribution.WebApi.Tests.Application.Commands.PublishAlarm;

public class PublishAlarmCommandHandlerTests
{
	private const int PRIMARY_NURSE_ID = 10;
	private const int SECONDARY_NURSE_ID = 20;

	private readonly INurseRepository _nurseRepositoryMock;
	private readonly IPatientRepository _patientRepositoryMock;
	private readonly IAlarmRepository _alarmRepositoryMock;
	private readonly IMonitoredAlarmsService _monitoredAlarmsServiceMock;
	private readonly ILogger<PublishAlarmCommandHandler> _loggerMock;

	private readonly PublishAlarmCommandHandler _sut;

	public PublishAlarmCommandHandlerTests()
	{
		_nurseRepositoryMock = Substitute.For<INurseRepository>();
		_patientRepositoryMock = Substitute.For<IPatientRepository>();
        _alarmRepositoryMock = Substitute.For<IAlarmRepository>();
		_monitoredAlarmsServiceMock = Substitute.For<IMonitoredAlarmsService>();
		_loggerMock = Substitute.For<ILogger<PublishAlarmCommandHandler>>();

		_sut = new(_nurseRepositoryMock, _patientRepositoryMock, _alarmRepositoryMock, _monitoredAlarmsServiceMock, _loggerMock);
    }

	#region Handle tests

	[Fact]
	public async Task Handle_WhenPatientNotFound_ThrowsPatientNotFoundException()
	{
		// Arrange
		var command = CreateTestCommand();
		_patientRepositoryMock.GetByIdAsync(command.PatientId, true, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Patient?>(null));

		// Act & Assert
		var exception = await Assert.ThrowsAsync<PatientNotFoundException>(
			async () => await _sut.Handle(command, CancellationToken.None));

		Assert.Equal(command.PatientId, exception.PatientId);
	}

	[Fact]
	public async Task Handle_WhenCalled_AddsAlarmToRepository()
	{
		// Arrange
		var command = CreateTestCommand();
		var cancellationToken = CancellationToken.None;

        // Act
		try
		{
			await _sut.Handle(command, cancellationToken);
		}
		catch (PatientNotFoundException)
		{
            // Ignored, as we're only interested in verifying that the alarm was added to the repository
        }

        // Assert
        await _alarmRepositoryMock.Received(1).AddAsync(
			Arg.Is<Alarm>(a => a.Id == command.AlarmId && a.PatientId == command.PatientId && a.Type == command.AlarmType),
            cancellationToken);
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseFound_NotifiesPrimaryNurse()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();
		SetupNurseFound(PRIMARY_NURSE_ID);
		var cancellationToken = CancellationToken.None;

		// Act
		await _sut.Handle(command, cancellationToken);

		// Assert
		await _nurseRepositoryMock.Received(1).UpdateAsync(
			Arg.Is<Nurse>(n => n.Id == PRIMARY_NURSE_ID && n.PendingAlarms.Contains(command.AlarmId)),
            cancellationToken);
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseFound_StartsAlarmMonitoring()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();
		SetupNurseFound(PRIMARY_NURSE_ID);

        // Act
        await _sut.Handle(command, CancellationToken.None);

		// Assert
		_monitoredAlarmsServiceMock.Received(1).StartAlarmMonitoring(
			Arg.Is<Alarm>(a => a.Id == command.AlarmId),
			Arg.Any<Func<AlarmMonitor, Task>>());
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseFound_DoesNotDirectlyNotifySecondaryNurse()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();
		SetupNurseFound(PRIMARY_NURSE_ID);

		// Act
		await _sut.Handle(command, CancellationToken.None);

		// Assert
		await _nurseRepositoryMock.DidNotReceive().GetByIdAsync(SECONDARY_NURSE_ID, Arg.Any<bool>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseNotFound_NotifiesSecondaryNurse()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();
		SetupNurseFound(SECONDARY_NURSE_ID);
		var cancellationToken = CancellationToken.None;

		// Act
		await _sut.Handle(command, CancellationToken.None);

		// Assert
		await _nurseRepositoryMock.Received(1).UpdateAsync(
			Arg.Is<Nurse>(n => n.Id == SECONDARY_NURSE_ID && n.PendingAlarms.Contains(command.AlarmId)),
			cancellationToken);
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseNotFound_DoesNotStartAlarmMonitoring()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();
		SetupNurseFound(SECONDARY_NURSE_ID);

		// Act
		await _sut.Handle(command, CancellationToken.None);

		// Assert
		_monitoredAlarmsServiceMock.DidNotReceive().StartAlarmMonitoring(Arg.Any<Alarm>(), Arg.Any<Func<AlarmMonitor, Task>>());
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseNotFoundAndSecondaryNurseNotFound_ThrowsNurseNotFoundException()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();

		// Act & Assert
		var exception = await Assert.ThrowsAsync<NurseNotFoundException>(
			async () => await _sut.Handle(command, CancellationToken.None));

		Assert.Equal(SECONDARY_NURSE_ID, exception.NurseId);
	}

	#endregion

	#region Helpers

	public static PublishAlarmCommand CreateTestCommand()
	{
		return new PublishAlarmCommand
		{
			AlarmId = 1,
			PatientId = 1,
			AlarmType = AlarmType.OxygenSaturation,
			Timestamp = DateTime.UtcNow
		};
	}


	private void SetupPatientFound()
	{
		var patient = CreateTestPatient();
		_patientRepositoryMock.GetByIdAsync(patient.Id, true, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Patient?>(patient));
	}

	private void SetupNurseFound(int nurseId)
	{
		var nurse = CreateTestNurse(nurseId);
		_nurseRepositoryMock.GetByIdAsync(nurseId, false, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Nurse?>(nurse));
	}

	private static Patient CreateTestPatient() =>
		new(1, "Test Patient", PRIMARY_NURSE_ID, SECONDARY_NURSE_ID);

	private static Nurse CreateTestNurse(int nurseId) =>
		new(nurseId, "Test Nurse");

	#endregion
}
