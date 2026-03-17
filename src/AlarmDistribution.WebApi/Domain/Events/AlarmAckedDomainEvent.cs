using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;
using Mediator;

namespace AlarmDistribution.WebApi.Domain.Events;

public class AlarmAckedDomainEvent : DomainEventBase, INotification
{
    public AlarmAckedDomainEvent(int alarmId, int patientId, int nurseId)
    {
        ArgumentOutOfRangeException.ThrowIfZero(alarmId);
        ArgumentOutOfRangeException.ThrowIfZero(patientId);
        ArgumentOutOfRangeException.ThrowIfZero(nurseId);

        AlarmId = alarmId;
        PatientId = patientId;
        NurseId = nurseId;
    }

    public int AlarmId { get; }
    public int PatientId { get; }
    public int NurseId { get; }
}
