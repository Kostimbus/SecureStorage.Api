using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureStorage.Core.Interfaces;
using SecureStorage.Core.Models;
using SecureStorage.Infrastructure.Data;

namespace SecureStorage.Infrastructure.Repositories
{
    public class EfFileRepository : IFileRepository
    {
        private readonly AppDbContext _context;

        public EfFileRepository(AppDbContext context)
            => _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<Guid> CreateAsync(FileRecord record, CancellationToken cancellationToken = default)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            _context.FileRecords.Add(record);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return record.Id;
        }

        public async Task<FileRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.FileRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<FileRecord>> ListByOwnerAsync(Guid ownerId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 50;

            return await _context.FileRecords
                .AsNoTracking()
                .Where(f => f.OwnerId == ownerId)
                .OrderByDescending(f => f.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task UpdateAsync(FileRecord record, CancellationToken cancellationToken = default)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            // Approach: attach the provided record and mark it modified.
            // This assumes the provided object contains the desired final state.
            _context.FileRecords.Update(record);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existing = await _context.FileRecords.FindAsync(new object?[] { id }, cancellationToken).ConfigureAwait(false);
            if (existing == null) return false;

            _context.FileRecords.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> DeleteAsync(FileRecord record, CancellationToken ct = default)
        {
            _context.FileRecords.Remove(record);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
