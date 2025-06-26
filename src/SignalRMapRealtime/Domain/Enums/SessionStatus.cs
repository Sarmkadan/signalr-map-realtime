// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SignalRMapRealtime.Domain.Enums;

/// <summary>
/// Represents the state of a tracking session.
/// </summary>
public enum SessionStatus
{
    /// <summary>Session has been initialized but not started.</summary>
    Pending = 0,

    /// <summary>Session is currently active and tracking.</summary>
    Active = 1,

    /// <summary>Session has been paused temporarily.</summary>
    Paused = 2,

    /// <summary>Session has been completed normally.</summary>
    Completed = 3,

    /// <summary>Session was terminated abnormally.</summary>
    Cancelled = 4,

    /// <summary>Session has expired due to timeout.</summary>
    Expired = 5
}
