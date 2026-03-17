namespace AlarmDistribution.WebApi.Application.Exceptions;

public class PatientNotFoundException : Exception
{
    public PatientNotFoundException(int patientId) 
        : base($"Patient with ID {patientId} not found.")
    {
        PatientId = patientId;
    }

    public int PatientId { get; }
}
