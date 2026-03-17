namespace AlarmDistribution.WebApi.Application.Models;

public class NurseResponse
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required IReadOnlyList<int> PendingAlarms { get; init; }
}
