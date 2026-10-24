using Microsoft.Extensions.Logging;
using NSubstitute;

namespace SignalRMapRealtime.Tests;

/// <summary>
/// Helpers for asserting on log output produced by NSubstitute logger mocks.
/// </summary>
public static class LoggerTestExtensions
{
    /// <summary>
    /// Determines whether the logger received a call at the given level whose formatted message contains the fragment.
    /// </summary>
    /// <remarks>
    /// The LogInformation/LogDebug/... extension methods call <c>ILogger.Log&lt;TState&gt;</c> with a formatter delegate
    /// that NSubstitute cannot match, so the formatted text is checked through <c>ReceivedCalls</c> instead.
    /// </remarks>
    /// <typeparam name="T">The logger category type.</typeparam>
    /// <param name="logger">The logger substitute.</param>
    /// <param name="level">The expected log level.</param>
    /// <param name="messageFragment">A substring that the formatted message must contain.</param>
    /// <returns><see langword="true"/> if a matching call was received; otherwise, <see langword="false"/>.</returns>
    public static bool HasLogged<T>(this ILogger<T> logger, LogLevel level, string messageFragment)
    {
        return logger.ReceivedCalls().Any(call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log)
            && call.GetArguments()[0] is LogLevel callLevel && callLevel == level
            && call.GetArguments()[2]?.ToString()?.Contains(messageFragment, StringComparison.Ordinal) == true);
    }
}
