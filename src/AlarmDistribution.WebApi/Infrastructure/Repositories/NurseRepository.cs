using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace AlarmDistribution.WebApi.Infrastructure.Repositories;

public class NurseRepository : INurseRepository
{
    private readonly AppDbContext _dbContext;

    public NurseRepository(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid nurseId, CancellationToken cancellationToken)
    {
        return _dbContext.Nurses.AsNoTracking().AnyAsync(n => n.Id == nurseId, cancellationToken);
    }

    public Task<List<Nurse>> GetAllAsync(bool readOnly, CancellationToken cancellationToken = default)
    {
        return readOnly 
            ? _dbContext.Nurses.AsNoTracking().ToListAsync(cancellationToken) 
            : _dbContext.Nurses.ToListAsync(cancellationToken);
    }

    public Task<Nurse?> GetByIdAsync(Guid nurseId, bool readOnly, CancellationToken cancellationToken = default)
    {
        return readOnly
            ? _dbContext.Nurses.AsNoTracking().FirstOrDefaultAsync(n => n.Id == nurseId, cancellationToken)
            : _dbContext.Nurses.FirstOrDefaultAsync(n => n.Id == nurseId, cancellationToken);
    }

    public Task UpdateAsync(Nurse nurse, CancellationToken cancellationToken = default)
    {
        _dbContext.Update(nurse);

        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
