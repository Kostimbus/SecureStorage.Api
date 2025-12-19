using System;
using System.Collections.Generic;
using System.Text;
using SecureStorage.Application.Interfaces;
using SecureStorage.Core.Interfaces;
using SecureStorage.Core.Models;

namespace SecureStorage.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;
        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }
        public async Task LogAsync(AuditEntry entry, CancellationToken ct = default)
        {
            await _auditRepository.AddAsync(entry, ct);
        }
        public async Task LogEventAsync(
            AuditEventType eventType,
            Guid? actorUserId = null,
            string? actorUsername = null,
            string? details = null,
            Guid? fileId = null,
            string? remoteIp = null,
            CancellationToken ct = default)
        {
            var entry = new AuditEntry
            {
                EventType = eventType,
                ActorUserId = actorUserId,
                ActorUsername = actorUsername,
                Details = details,
                FileId = fileId,
                RemoteIp = remoteIp,
                OccurredAtUtc = DateTime.UtcNow
            };
            await _auditRepository.AddAsync(entry, ct);
        }
    }
}
