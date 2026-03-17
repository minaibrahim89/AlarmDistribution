using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace AlarmDistribution.WebApi.Infrastructure.Repositories;

public class AlarmRepository : IAlarmRepository
{
    private readonly AppDbContext _dbContext;

    public AlarmRepository(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Task<Alarm?> GetAlarmById(Guid alarmId, bool readOnly, CancellationToken cancellationToken = default)
    {
        return readOnly
            ? _dbContext.Alarms.AsNoTracking().FirstOrDefaultAsync(a => a.Id == alarmId, cancellationToken)
            : _dbContext.Alarms.FirstOrDefaultAsync(a => a.Id == alarmId, cancellationToken);
    }

    public Task AddAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        _dbContext.Alarms.Add(alarm);

        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(Alarm alarm, CancellationToken cancellationToken = default)
    {
        _dbContext.Update(alarm);

        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
