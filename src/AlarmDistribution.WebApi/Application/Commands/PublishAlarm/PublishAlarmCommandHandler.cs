using AlarmDistribution.WebApi.Application.Exceptions;
using AlarmDistribution.WebApi.Application.Services;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using Mediator;

namespace AlarmDistribution.WebApi.Application.Commands.PublishAlarm;

public class PublishAlarmCommandHandler : IRequestHandler<PublishAlarmCommand>
{
    private readonly INurseRepository _nurseRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IAlarmRepository _alarmRepository;
    private readonly IMonitoredAlarmsService _monitoredAlarmsService;
    private readonly ILogger<PublishAlarmCommandHandler> _logger;

    public PublishAlarmCommandHandler(
        INurseRepository nurseRepository,
        IPatientRepository patientRepository,
        IAlarmRepository alarmRepository,
        IMonitoredAlarmsService monitoredAlarmsService,
        ILogger<PublishAlarmCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(nurseRepository);
        ArgumentNullException.ThrowIfNull(patientRepository);
        ArgumentNullException.ThrowIfNull(alarmRepository);
        ArgumentNullException.ThrowIfNull(monitoredAlarmsService);
        ArgumentNullException.ThrowIfNull(logger);

        _nurseRepository = nurseRepository;
        _patientRepository = patientRepository;
        _alarmRepository = alarmRepository;
        _monitoredAlarmsService = monitoredAlarmsService;
        _logger = logger;
    }

    public async ValueTask<Unit> Handle(PublishAlarmCommand request, CancellationToken cancellationToken)
    {
        var alarm = new Alarm(request.AlarmId, request.PatientId, request.AlarmType, request.Timestamp);
            
        await _alarmRepository.AddAsync(alarm, cancellationToken);
        await NotifyPrimaryNurseAndMonitorAlarmForEscalationAsync(alarm, cancellationToken);
        return Unit.Value;
    }

    private async Task NotifyPrimaryNurseAndMonitorAlarmForEscalationAsync(Alarm alarm, CancellationToken cancellationToken)
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

        _monitoredAlarmsService.StartAlarmMonitoring(alarm);
    }

    private async Task<bool> NotifyNurseAsync(int nurseId, Alarm alarm, bool isPrimaryNurse, CancellationToken cancellationToken)
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
}