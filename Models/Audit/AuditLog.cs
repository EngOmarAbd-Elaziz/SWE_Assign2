namespace Masroofy.App.Models;

/// <summary>
/// Represents an audit log entry that records system or user actions
/// for tracking and monitoring purposes.
/// </summary>
public sealed class AuditLog
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Description of the action performed in the system.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the action was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
