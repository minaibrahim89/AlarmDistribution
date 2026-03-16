namespace AlarmDistribution.WebApi.Domain.Aggregates.Nurses;

public interface INurseRepository
{
    public Task<Nurse?> GetByIdAsync(Guid nurseId, bool readOnly, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(Guid nurseId, CancellationToken cancellationToken);

    public Task<List<Nurse>> GetAllAsync(bool readOnly, CancellationToken cancellationToken = default);

    public Task UpdateAsync(Nurse nurse, CancellationToken cancellationToken = default);

}
