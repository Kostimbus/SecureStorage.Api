using System;

namespace SecureStorage.Api.DTOs
{
    public class AdminAuditDto
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public Guid? ActorUserId { get; set; }
        public string? ActorUsername { get; set; }
        public Guid? FileId { get; set; }
        public string? Details { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string? RemoteIp { get; set; }
    }
}
