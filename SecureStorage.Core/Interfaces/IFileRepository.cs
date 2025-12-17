using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureStorage.Core.Models;

namespace SecureStorage.Core.Interfaces
{
    public interface IFileRepository
    {
        /// <summary>
        /// Persists a new file record and returns its generated Id.
        /// </summary>
        Task<Guid> CreateAsync(FileRecord record, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a file record by its id, or null if not found.
        /// </summary>
        Task<FileRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists file records that belong to a specific user (owner).
        /// Pagination is simple: page is 1-based.
        /// </summary>
        Task<IEnumerable<FileRecord>> ListByOwnerAsync(Guid ownerId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing file record. The record.Id must be set.
        /// </summary>
        Task UpdateAsync(FileRecord record, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file record by id. Returns true if record existed and was deleted.
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file record. Returns true if record existed and was deleted.
        /// </summary>
        Task<bool> DeleteAsync(FileRecord record, CancellationToken ct = default);

        /// <summary>
        /// List all files with their owners and total count. For admin use only.
        /// </summary>
        Task<(IList<FileRecordWithOwner> Items, int TotalCount)> ListAllWithOwnerAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    }
}
