using AlarmDistribution.WebApi.Application.Behaviors;
using AlarmDistribution.WebApi.Application.Services;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using AlarmDistribution.WebApi.Infrastructure.Database;
using AlarmDistribution.WebApi.Infrastructure.Repositories;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AlarmDistribution.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        return services
            .AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionPipelineBehavior<,>))
            .AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>()
            .AddScoped<IAlarmNotificationService, AlarmNotificationService>()
            .AddSingleton<IMonitoredAlarmsService, MonitoredAlarmsService>()
            .AddRepositories();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("AlarmDistributionDb"));

        services.AddScoped<IAlarmRepository, AlarmRepository>();
        services.AddScoped<INurseRepository, NurseRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }
}
