using System.Collections.Concurrent;
using AlarmDistribution.WebApi.Application.Exceptions;
using AlarmDistribution.WebApi.Application.Model;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;

namespace AlarmDistribution.WebApi.Application.Services;

public class AlarmNotificationService : IAlarmNotificationService
{
    private readonly IMonitoredAlarmsService _monitoredAlarmsService;
    private readonly IPatientRepository _patientRepository;
    private readonly INurseRepository _nurseRepository;
    private readonly ILogger<AlarmMonitor> _alarmMonitorLogger;
    private readonly ILogger<AlarmNotificationService> _logger;

    public AlarmNotificationService(
        IMonitoredAlarmsService monitoredAlarmsService,
        IPatientRepository patientRepository,
        INurseRepository nurseRepository,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(monitoredAlarmsService);
        ArgumentNullException.ThrowIfNull(patientRepository);
        ArgumentNullException.ThrowIfNull(nurseRepository);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _monitoredAlarmsService = monitoredAlarmsService;
        _patientRepository = patientRepository;
        _nurseRepository = nurseRepository;
        _alarmMonitorLogger = loggerFactory.CreateLogger<AlarmMonitor>();
        _logger = loggerFactory.CreateLogger<AlarmNotificationService>();
    }

    public async Task NotifyPrimaryNurseAndMonitorAlarmForEscalationAsync(Alarm alarm, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(alarm);

        var patient = await _patientRepository.GetByIdAsync(alarm.PatientId, true, cancellationToken);

        if (patient == null)
            throw new PatientNotFoundException(alarm.PatientId);

        var primaryNurseNotified = await NotifyNurseAsync(patient.PrimaryNurseId, alarm, true, cancellationToken);

        if (!primaryNurseNotified)
        {
            // Immediately notify secondary nurse if primary nurse is not found, as we cannot rely on escalation in this case
            await NotifyNurseAsync(patient.SecondaryNurseId, alarm, false, cancellationToken);
            return;
        }

        _monitoredAlarmsService.StartAlarmMonitoring(alarm, (monitor) => NotifySecondaryNurse(monitor, patient.SecondaryNurseId));
    }

    private async Task NotifySecondaryNurse(AlarmMonitor alarmMonitor, Guid secondaryNurseId)
    {
        try
        {
            _logger.LogInformation("Escalating alarm with ID {AlarmId} to secondary nurse with ID {NurseId}", alarmMonitor.Alarm.Id, secondaryNurseId);

            await NotifyNurseAsync(secondaryNurseId, alarmMonitor.Alarm, false, CancellationToken.None);

            _logger.LogInformation("Successfully escalated alarm with ID {AlarmId} to secondary nurse with ID {NurseId}", alarmMonitor.Alarm.Id, secondaryNurseId);
        }
        finally
        {
            CancelEscalationMonitoring(alarmMonitor.Alarm.Id);
        }
    }

    private async Task<bool> NotifyNurseAsync(Guid nurseId, Alarm alarm, bool isPrimaryNurse, CancellationToken cancellationToken)
    {
        var nurse = await _nurseRepository.GetByIdAsync(nurseId, false, cancellationToken);

        if (nurse == null)
        {
            if (!isPrimaryNurse)
                throw new NurseNotFoundException(nurseId);

            _logger.LogWarning("Primary nurse with ID {NurseId} not found when notifying about alarm with ID {AlarmId}", nurseId, alarm.Id);
            return false;
        }

        nurse.NotifyAlarm(alarm.Id);
        await _nurseRepository.UpdateAsync(nurse, cancellationToken);

        return true;
    }

    public void CancelEscalationMonitoring(Guid alarmId)
    {
        _monitoredAlarmsService.StopAlarmMonitoring(alarmId);
    }
}
