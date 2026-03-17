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
	private readonly INurseRepository _nurseRepositoryMock;
	private readonly IPatientRepository _patientRepositoryMock;
	private readonly IAlarmRepository _alarmRepositoryMock;
	private readonly IMonitoredAlarmsService _monitoredAlarmsServiceMock;
	private readonly ILogger<PublishAlarmCommandHandler> _loggerMock;

	private readonly PublishAlarmCommandHandler _sut;

	private const int PrimaryNurseId = 10;
	private const int SecondaryNurseId = 20;

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
		SetupNurseFound(PrimaryNurseId);
		var cancellationToken = CancellationToken.None;

		// Act
		await _sut.Handle(command, cancellationToken);

		// Assert
		await _nurseRepositoryMock.Received(1).UpdateAsync(
			Arg.Is<Nurse>(n => n.Id == PrimaryNurseId && n.PendingAlarms.Contains(command.AlarmId)),
            cancellationToken);
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseFound_StartsAlarmMonitoring()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();
		SetupNurseFound(PrimaryNurseId);

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
		SetupNurseFound(PrimaryNurseId);

		// Act
		await _sut.Handle(command, CancellationToken.None);

		// Assert
		await _nurseRepositoryMock.DidNotReceive().GetByIdAsync(SecondaryNurseId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseNotFound_NotifiesSecondaryNurse()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();
		SetupNurseNotFound(PrimaryNurseId);
		SetupNurseFound(SecondaryNurseId);
		var cancellationToken = CancellationToken.None;

		// Act
		await _sut.Handle(command, CancellationToken.None);

		// Assert
		await _nurseRepositoryMock.Received(1).UpdateAsync(
			Arg.Is<Nurse>(n => n.Id == SecondaryNurseId && n.PendingAlarms.Contains(command.AlarmId)),
			cancellationToken);
	}

	[Fact]
	public async Task Handle_WhenPrimaryNurseNotFound_DoesNotStartAlarmMonitoring()
	{
		// Arrange
		var command = CreateTestCommand();
		SetupPatientFound();
		SetupNurseNotFound(PrimaryNurseId);
		SetupNurseFound(SecondaryNurseId);

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
		SetupNurseNotFound(PrimaryNurseId);
		SetupNurseNotFound(SecondaryNurseId);

		// Act & Assert
		var exception = await Assert.ThrowsAsync<NurseNotFoundException>(
			async () => await _sut.Handle(command, CancellationToken.None));

		Assert.Equal(SecondaryNurseId, exception.NurseId);
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

	private static Patient CreateTestPatient() =>
		new(1, "Test Patient", PrimaryNurseId, SecondaryNurseId);

	private Patient SetupPatientFound()
	{
		var patient = CreateTestPatient();
		_patientRepositoryMock.GetByIdAsync(patient.Id, true, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Patient?>(patient));
		return patient;
	}

	private Nurse SetupNurseFound(int nurseId)
	{
		var nurse = new Nurse(nurseId, "Test Nurse");
		_nurseRepositoryMock.GetByIdAsync(nurseId, false, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Nurse?>(nurse));
		return nurse;
	}

	private void SetupNurseNotFound(int nurseId) =>
		_nurseRepositoryMock.GetByIdAsync(nurseId, false, Arg.Any<CancellationToken>()).Returns(Task.FromResult<Nurse?>(null));

	#endregion
}
