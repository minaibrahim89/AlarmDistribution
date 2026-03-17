using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Domain.Aggregates.Patients;

public class Patient : EntityBase<int>
{
    // For EF Core
    public Patient()
    {        
    }

    public Patient(int id, string name, int primaryNurseId, int secondaryNurseId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(primaryNurseId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(secondaryNurseId);

        Id = id;
        Name = name;
        PrimaryNurseId = primaryNurseId;
        SecondaryNurseId = secondaryNurseId;
    }

    public string Name { get; }
    public int PrimaryNurseId { get; }
    public int SecondaryNurseId { get; }
}
