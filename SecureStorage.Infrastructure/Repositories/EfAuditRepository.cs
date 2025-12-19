using System;
using System.Collections.Generic;
using System.Text;
using SecureStorage.Core.Models;
using SecureStorage.Core.Interfaces;
using SecureStorage.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SecureStorage.Infrastructure.Repositories
{
    public class EfAuditRepository : IAuditRepository
    {
        private readonly AppDbContext _dbContext;
        public EfAuditRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(AuditEntry entry, CancellationToken ct = default)
        {
            _dbContext.AuditEntries.Add(entry);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<(IList<AuditEntry> Items, int TotalCount)> QueryAsync(
            int page = 1,
            int pageSize = 50,
            AuditEventType? eventType = null,
            string? actorUsername = null,
            DateTime? dateFromUtc = null,
            DateTime? dateToUtc = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;

            var query = _dbContext.AuditEntries.AsNoTracking().AsQueryable();

            if (eventType.HasValue)
            {
                query = query.Where(a => a.EventType == eventType.Value);
            }

            if (!string.IsNullOrWhiteSpace(actorUsername))
            {
                var normalized = actorUsername.Trim();
                query = query.Where(a => a.ActorUsername != null && a.ActorUsername.Contains(normalized));
            }

            if (dateFromUtc.HasValue)
            {
                query = query.Where(a => a.OccurredAtUtc >= dateFromUtc.Value);
            }

            if (dateToUtc.HasValue)
            {
                query = query.Where(a => a.OccurredAtUtc <= dateToUtc.Value);
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(a => a.OccurredAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
