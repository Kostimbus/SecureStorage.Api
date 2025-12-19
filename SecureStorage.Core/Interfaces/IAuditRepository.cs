using System;
using System.Collections.Generic;
using System.Text;
using SecureStorage.Core.Models;

namespace SecureStorage.Core.Interfaces
{
    public interface IAuditRepository
    {
        Task AddAsync(AuditEntry entry, CancellationToken ct = default);

        //Paged query
        Task<(IList<AuditEntry> Items, int TotalCount)> QueryAsync(
            int page = 1,
            int pageSize = 50,
            AuditEventType? eventType = null,
            string? actorUsername = null,
            DateTime? dateFromUtc = null,
            DateTime? dateToUtc = null,
            CancellationToken ct = default);
    }
}
