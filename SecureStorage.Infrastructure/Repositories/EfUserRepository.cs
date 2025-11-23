using SecureStorage.Core.Interfaces;
using SecureStorage.Infrastructure.Data;
using SecureStorage.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SecureStorage.Infrastructure.Repositories
{
    public class EfUserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public EfUserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return user.Id;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            var norm = username.Trim().ToLowerInvariant();
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username.ToLower() == norm, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var norm = email.Trim().ToLowerInvariant();
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == norm, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            _context.Users.Update(user);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Users.FindAsync(new object?[] { id }, cancellationToken).ConfigureAwait(false);
            if (existing == null) return false;

            _context.Users.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
    }
}
