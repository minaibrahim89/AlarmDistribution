using AlarmDistribution.WebApi.Application.Behaviors;
using AlarmDistribution.WebApi.Application.Services;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using AlarmDistribution.WebApi.Infrastructure.Database.Context;
using AlarmDistribution.WebApi.Infrastructure.Repositories;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace AlarmDistribution.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionPipelineBehavior<,>))
            .AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>()
            .AddScoped<IMonitoredAlarmsService, MonitoredAlarmsService>()
            .AddSingleton<IMonitoredAlarmsService, MonitoredAlarmsService>()
            .AddRepositories(configuration);
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AlarmDistributionDb")
            ?? "Data Source=AlarmDistribution.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAlarmRepository, AlarmRepository>();
        services.AddScoped<INurseRepository, NurseRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }
}
