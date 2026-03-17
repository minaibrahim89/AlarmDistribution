namespace AlarmDistribution.WebApi.Domain.Aggregates.Patients;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(int patientId, bool readOnly, CancellationToken cancellationToken = default);
}
