namespace AlarmDistribution.WebApi.Domain.Aggregates.Patients;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid patientId, bool readOnly, CancellationToken cancellationToken = default);
}
