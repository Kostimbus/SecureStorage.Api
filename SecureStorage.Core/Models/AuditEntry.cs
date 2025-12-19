using System;
using System.Collections.Generic;
using System.Text;

namespace SecureStorage.Core.Models
{
    /// <summary>
    /// Represents an immutable audit record for a user action.
    /// </summary>
    public class AuditEntry
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public AuditEventType EventType { get; init; }
        public Guid? ActorUserId { get; init; }
        public string? ActorUsername { get; init; }
        public string? Details { get; init; }
        public Guid? FileId { get; init; }
        public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
        public string? RemoteIp { get; init; }
    }
}
