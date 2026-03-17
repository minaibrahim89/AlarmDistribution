namespace AlarmDistribution.WebApi.Tests.Helpers;

/// <summary>
/// A test double for <see cref="TimeProvider"/> that allows controlling the current time
/// while delegating timer creation to the real base implementation so real timers fire.
/// </summary>
internal sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public void SetUtcNow(DateTimeOffset utcNow) => _utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => _utcNow;
}
