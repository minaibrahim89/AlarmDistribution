using AlarmDistribution.WebApi.Application.Exceptions;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using Mediator;

namespace AlarmDistribution.WebApi.Application.Commands.AckAlarm;

public class AckAlarmCommandHandler : IRequestHandler<AckAlarmCommand>
{
    private readonly IAlarmRepository _alarmRepository;
    private readonly INurseRepository _nurseRepository;

    public AckAlarmCommandHandler(IAlarmRepository alarmRepository, INurseRepository nurseRepository)
    {
        ArgumentNullException.ThrowIfNull(alarmRepository);
        ArgumentNullException.ThrowIfNull(nurseRepository);

        _alarmRepository = alarmRepository;
        _nurseRepository = nurseRepository;
    }

    public async ValueTask<Unit> Handle(AckAlarmCommand request, CancellationToken cancellationToken)
    {
        var alarm = await _alarmRepository.GetAlarmById(request.AlarmId, false, cancellationToken);

        if (alarm == null)
            throw new AlarmNotFoundException(request.AlarmId);  

        var nurse = await _nurseRepository.GetByIdAsync(request.NurseId, false, cancellationToken);
        
        if (nurse is null)
            throw new NurseNotFoundException(request.NurseId);

        if (!nurse.PendingAlarms.Contains(alarm.Id))
            throw new AlarmNotAssignedToNurseException(alarm.Id, nurse.Id);

        alarm.Acknowledge(request.NurseId);
        await _alarmRepository.UpdateAsync(alarm, cancellationToken);

        return Unit.Value;
    }
}
