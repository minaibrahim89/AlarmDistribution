namespace AlarmDistribution.WebApi.Application.Exceptions;

public class NurseNotFoundException : Exception
{
    public NurseNotFoundException(int nurseId) 
        : base($"Nurse with id {nurseId} is not found")
    {
        NurseId = nurseId;
    }

    public int NurseId { get; }
}
