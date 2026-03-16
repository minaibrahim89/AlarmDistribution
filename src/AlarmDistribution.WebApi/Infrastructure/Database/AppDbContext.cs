using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using Ardalis.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace AlarmDistribution.WebApi.Infrastructure.Database;

public class AppDbContext : DbContext
{
    private readonly IDomainEventDispatcher _dispatcher;

    public AppDbContext(DbContextOptions dbContextOptions, IDomainEventDispatcher dispatcher)
        : base(dbContextOptions)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        _dispatcher = dispatcher;
    }

    public DbSet<Alarm> Alarms { get; set; }

    public DbSet<Nurse> Nurses { get; set; }

    public DbSet<Patient> Patients { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);

        var domainEvents = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(e => e.Entity);

        await _dispatcher.DispatchAndClearEvents(domainEvents);

        return result;
    }
}
