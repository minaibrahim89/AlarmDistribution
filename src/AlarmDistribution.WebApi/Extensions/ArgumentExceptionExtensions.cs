using System.Runtime.CompilerServices;

namespace AlarmDistribution.WebApi.Extensions;

public static class ArgumentExceptionExtensions
{
    extension(ArgumentException)
    {
        public static void ThrowIfEmpty(Guid guid, [CallerArgumentExpression("guid")]string? parameterName = null)
        {
            if (guid == Guid.Empty)
                throw new ArgumentException("Value cannot be empty", parameterName);
        }

        public static void ThrowIfEmpty(DateTimeOffset timestamp, [CallerArgumentExpression("timestamp")]string? parameterName = null)
        {
            if (timestamp == default)
                throw new ArgumentException("Value cannot be empty", parameterName);
        }
    }
}
