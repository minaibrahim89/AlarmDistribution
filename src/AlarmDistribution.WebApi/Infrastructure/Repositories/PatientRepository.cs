using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using AlarmDistribution.WebApi.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace AlarmDistribution.WebApi.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _dbContext;

    public PatientRepository(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Task<Patient?> GetByIdAsync(Guid patientId, bool readOnly, CancellationToken cancellationToken = default)
    {
        return readOnly
            ? _dbContext.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken)
            : _dbContext.Patients.FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
    }
}
