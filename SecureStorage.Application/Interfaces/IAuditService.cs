using System;
using System.Collections.Generic;
using System.Text;
using SecureStorage.Core.Models;

namespace SecureStorage.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditEntry entry, CancellationToken ct = default);

        Task LogEventAsync(
            AuditEventType eventType,
            Guid? actorUserId = null,
            string? actorUsername = null,
            string? details = null,
            Guid? fileId = null,
            string? remoteIp = null,
            CancellationToken ct = default);
    }
}
