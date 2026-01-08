using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureStorage.Application.Interfaces;
using SecureStorage.Application.Services;
using SecureStorage.Core.Interfaces;
using SecureStorage.Infrastructure.Crypto;
using SecureStorage.Infrastructure.Data;
using SecureStorage.Infrastructure.Repositories;
using System;

namespace SecureStorage.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        /// <summary>
        /// Adds infrastructure services: DbContext, repositories and crypto.
        /// Expects connection string named "Default" and config section "Encryption:Base64Key".
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // Configure options
            services.Configure<AesGcmOptions>(configuration.GetSection("Encryption"));

            // DbContext: choose provider via configuration - here we attempt to infer SQLite if DataSource/Filename present
            var conn = configuration.GetConnectionString("Default");
            if (string.IsNullOrWhiteSpace(conn))
            {
                throw new InvalidOperationException("Connection string 'Default' is not configured.");
            }

            // Simple heuristic: if connection string contains "Filename=" or "Data Source=" use Sqlite
            if (conn.Contains("Filename=", StringComparison.OrdinalIgnoreCase) ||
                conn.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
                conn.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(conn));
            }
            else
            {
                // Default to Npgsql (Postgres). If you use SQL Server, change to UseSqlServer.
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(conn));
            }

            // Repositories & services
            services.AddScoped<IFileRepository, EfFileRepository>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IAuditRepository, EfAuditRepository>();
            services.AddScoped<IFileStorage, LocalFileStorage>();

            // Encryption service: singleton is OK because it only holds the key bytes (thread-safe).
            services.AddSingleton<IFileEncryptionService, AesGcmFileEncryptionService>();

            // register JWT token service
            services.AddSingleton<IJwtTokenService, SecureStorage.Infrastructure.Security.JwtTokenService>();

            return services;

        }
    }
}
