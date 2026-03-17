namespace AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

public interface IAlarmRepository
{
    Task<Alarm?> GetAlarmById(int alarmId, bool readOnly, CancellationToken cancellationToken = default);

    Task AddAsync(Alarm alarm, CancellationToken cancellationToken = default);

    Task UpdateAsync(Alarm alarm, CancellationToken cancellationToken = default);
}
