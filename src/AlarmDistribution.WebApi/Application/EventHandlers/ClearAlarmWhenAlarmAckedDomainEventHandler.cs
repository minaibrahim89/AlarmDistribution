using AlarmDistribution.WebApi.Application.Services;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using AlarmDistribution.WebApi.Domain.Events;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Application.EventHandler;

public class ClearAlarmWhenAlarmAckedDomainEventHandler : IDomainEventHandler<AlarmAckedDomainEvent>
{
    private readonly INurseRepository _nurseRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IMonitoredAlarmsService _monitoredAlarmsService;
    private readonly ILogger<ClearAlarmWhenAlarmAckedDomainEventHandler> _logger;

    public ClearAlarmWhenAlarmAckedDomainEventHandler(
        INurseRepository nurseRepository,
        IPatientRepository patientRepository,
        IMonitoredAlarmsService monitoredAlarmsService,
        ILogger<ClearAlarmWhenAlarmAckedDomainEventHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(nurseRepository);
        ArgumentNullException.ThrowIfNull(patientRepository);
        ArgumentNullException.ThrowIfNull(monitoredAlarmsService);
        ArgumentNullException.ThrowIfNull(logger);

        _nurseRepository = nurseRepository;
        _patientRepository = patientRepository;
        _monitoredAlarmsService = monitoredAlarmsService;
        _logger = logger;
    }

    public async ValueTask Handle(AlarmAckedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Alarm with ID {AlarmId} acknowledged, cancelling escalation if active", notification.AlarmId);

        _monitoredAlarmsService.StopAlarmMonitoring(notification.AlarmId);

        var patient = await _patientRepository.GetByIdAsync(notification.PatientId, false, cancellationToken);

        if (patient is null)
        {
            _logger.LogWarning("Patient with ID {PatientId} not found while handling AlarmAckedDomainEvent for Alarm ID {AlarmId}", notification.PatientId, notification.AlarmId);
            return;
        }

        var primaryNurseTask = ClearAlarmForNurseAsync(patient.PrimaryNurseId, notification.AlarmId, cancellationToken);
        var secondaryNurseTask = ClearAlarmForNurseAsync(patient.SecondaryNurseId, notification.AlarmId, cancellationToken);

        await Task.WhenAll(primaryNurseTask, secondaryNurseTask);
    }

    private async Task ClearAlarmForNurseAsync(int nurseId, int alarmId, CancellationToken cancellationToken)
    {
        var nurse = await _nurseRepository.GetByIdAsync(nurseId, false, cancellationToken);

        if (nurse is null)
        {
            _logger.LogWarning("Nurse with ID {NurseId} not found while trying to clear alarm with ID {AlarmId}", nurseId, alarmId);
            return;
        }

        nurse.RemovePendingAlarm(alarmId);
        await _nurseRepository.UpdateAsync(nurse, cancellationToken);
    }
}
