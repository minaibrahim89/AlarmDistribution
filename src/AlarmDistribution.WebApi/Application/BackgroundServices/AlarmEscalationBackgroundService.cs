using System.Threading.Channels;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using AlarmDistribution.WebApi.Infrastructure.Database.Context;

namespace AlarmDistribution.WebApi.Application.BackgroundServices;

public class AlarmEscalationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Channel<Alarm> _escalatedAlarmsChannel;
    private readonly ILogger<AlarmEscalationBackgroundService> _logger;

    public AlarmEscalationBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        Channel<Alarm> escalatedAlarmsChannel,
        ILogger<AlarmEscalationBackgroundService> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        ArgumentNullException.ThrowIfNull(escalatedAlarmsChannel);
        ArgumentNullException.ThrowIfNull(logger);

        _serviceScopeFactory = serviceScopeFactory;
        _escalatedAlarmsChannel = escalatedAlarmsChannel;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var escalatedAlarm = await _escalatedAlarmsChannel.Reader.ReadAsync(stoppingToken);

            await NotifySecondaryNurseAsync(escalatedAlarm, stoppingToken);
        }
    }

    private async Task NotifySecondaryNurseAsync(Alarm escalatedAlarm, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var patientRepo = scope.ServiceProvider.GetRequiredService<IPatientRepository>();
        var nurseRepo = scope.ServiceProvider.GetRequiredService<INurseRepository>();

        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        var patient = await patientRepo.GetByIdAsync(escalatedAlarm.PatientId, true, cancellationToken);

        if (patient is null)
        {
            _logger.LogWarning("Patient with ID {PatientId} not found.", escalatedAlarm.PatientId);
            return;
        }

        var secondaryNurse = await nurseRepo.GetByIdAsync(patient.SecondaryNurseId, false, cancellationToken);

        if (secondaryNurse is null)
        {
            _logger.LogWarning("Secondary nurse with ID {NurseId} not found for patient ID {PatientId}.", patient.SecondaryNurseId, escalatedAlarm.PatientId);
            return;
        }

        _logger.LogInformation("Notifying secondary nurse with ID {NurseId} about escalated alarm with ID {AlarmId}.", secondaryNurse.Id, escalatedAlarm.Id);
        
        secondaryNurse.NotifyAlarm(escalatedAlarm.Id);
        await nurseRepo.UpdateAsync(secondaryNurse, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}