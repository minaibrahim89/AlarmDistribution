using AlarmDistribution.WebApi.Extensions;
using Ardalis.SharedKernel;

namespace AlarmDistribution.WebApi.Domain.Aggregates.Patients;

public class Patient : EntityBase<Guid>
{
    // For EF Core
    public Patient()
    {        
    }

    public Patient(Guid id, string name, Guid primaryNurseId, Guid secondaryNurseId)
    {
        ArgumentException.ThrowIfEmpty(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfEmpty(primaryNurseId);
        ArgumentException.ThrowIfEmpty(secondaryNurseId);

        Id = id;
        Name = name;
        PrimaryNurseId = primaryNurseId;
        SecondaryNurseId = secondaryNurseId;
    }

    public string Name { get; }
    public Guid PrimaryNurseId { get; }
    public Guid SecondaryNurseId { get; }
}
