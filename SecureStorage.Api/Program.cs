using SecureStorage.Application.Interfaces;
using SecureStorage.Application.Services;
using SecureStorage.Infrastructure.Crypto;
using SecureStorage.Infrastructure.Options;
using SecureStorage.Infrastructure.DependencyInjection;
using SecureStorage.Core.Interfaces;
using SecureStorage.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5149", "https://localhost:7149");

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.Configure<AesGcmOptions>(
    builder.Configuration.GetSection("Encryption"));
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("Storage"));


builder.Services.AddSingleton<AesGcmFileEncryptionService>();
builder.Services.AddScoped<IFileCryptoService, AesGcmFileCryptoService>();
builder.Services.AddScoped<IFileRepository, EfFileRepository>();
builder.Services.AddScoped<IFileService, FileService>();

// Add Infrastructure (DbContext, repos, encryption)
builder.Services.AddInfrastructure(builder.Configuration);

// Optional: configure JSON, max request size etc
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opts =>
{
    opts.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB, adapt as needed
});

// JWT options
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not set.");
var issuer = builder.Configuration["Jwt:Issuer"] ?? "SecureStorage";
var audience = builder.Configuration["Jwt:Audience"] ?? "SecureStorageClients";

var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // for development only; set true in prod
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateLifetime = true
    };
});

// role-based policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // migrate DB on startup (dev convenience)
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SecureStorage.Infrastructure.Data.AppDbContext>();
        db.Database.Migrate();

        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var anyAdmin = db.Users.Any(u => u.Role == "Admin");
        if (!anyAdmin)
        {
            // Only for DEV seed an initial admin
            var adminPass = builder.Configuration["AdminInitialPassword"];
            if (!string.IsNullOrWhiteSpace(adminPass))
            {
                var adminUser = new SecureStorage.Core.Models.User
                {
                    Username = "admin",
                    Email = "admin@localhost",
                    Role = "Admin"
                };
                _ = userService.CreateUserAsync(adminUser, adminPass).GetAwaiter().GetResult();
                Console.WriteLine("Seeded initial admin user 'admin' (password from InitialAdminPassword).");
            }
        }

    }
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
