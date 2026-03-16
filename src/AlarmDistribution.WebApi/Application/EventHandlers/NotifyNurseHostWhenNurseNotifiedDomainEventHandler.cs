using AlarmDistribution.WebApi.Domain.Events;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Application.EventHandler;

public class NotifyNurseHostWhenNurseNotifiedDomainEventHandler : IDomainEventHandler<NurseNotifiedDomainEvent>
{
    private readonly ILogger<NotifyNurseHostWhenNurseNotifiedDomainEventHandler> _logger;

    public NotifyNurseHostWhenNurseNotifiedDomainEventHandler(ILogger<NotifyNurseHostWhenNurseNotifiedDomainEventHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public ValueTask Handle(NurseNotifiedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Nurse with ID {NurseId} has been notified about alarm with ID {AlarmId}",
            notification.NurseId, notification.AlarmId);

        return ValueTask.CompletedTask;
    }
}
