namespace AlarmDistribution.WebApi.Application.Exceptions;

public class PatientNotFoundException : Exception
{
    public PatientNotFoundException(Guid patientId) 
        : base($"Patient with ID {patientId} not found.")
    {
        PatientId = patientId;
    }

    public Guid PatientId { get; }
}
