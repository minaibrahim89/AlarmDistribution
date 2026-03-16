namespace AlarmDistribution.WebApi.Application.Exceptions;

public class NurseNotFoundException : Exception
{
    public NurseNotFoundException(Guid nurseId) 
        : base($"Nurse with id {nurseId} is not found")
    {
        NurseId = nurseId;
    }

    public Guid NurseId { get; }
}
