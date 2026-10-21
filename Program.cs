using System.Reflection;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ONEERP.Platform.API.Controllers;
using ONEERP.Platform.API.Data;
using ONEERP.Platform.API.Middleware;
using ONEERP.Platform.API.Repositories;
using ONEERP.Platform.API.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/platform-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers(options =>
{
    // Server-side authorization for the high-privilege migration endpoints.
    options.Filters.Add<MigrationAuthorizationFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ONE ERP Platform API",
        Version = "v1",
        Description = "Multi-tenant SaaS platform: tenants, plans, subscriptions, database provisioning."
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and the JWT token."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddSingleton<MigrationCatalogSeeder>();
builder.Services.AddScoped<ISqlHelper, SqlHelper>();
builder.Services.AddScoped<IDatabaseProvisioningService, DatabaseProvisioningService>();
builder.Services.AddSingleton<IMigrationScriptProvider, MigrationScriptProvider>();
builder.Services.AddScoped<ITenantSchemaVersionStore, TenantSchemaVersionStore>();
builder.Services.AddSingleton<ITenantMigrationLock, TenantMigrationLock>();

builder.Services.AddScoped<IMigrationRepository, MigrationRepository>();
builder.Services.AddScoped<ITenantMigrationHistoryRepository, TenantMigrationHistoryRepository>();
builder.Services.AddScoped<IDemoDataScriptProvider, DemoDataScriptProvider>();
builder.Services.AddScoped<IPurchaseDemoDataScriptProvider, PurchaseDemoDataScriptProvider>();
builder.Services.AddScoped<ITenantDatabaseHealthService, TenantDatabaseHealthService>();
builder.Services.AddScoped<IDemoDataSeedingService, DemoDataSeedingService>();
builder.Services.AddScoped<IPurchaseDemoDataSeedingService, PurchaseDemoDataSeedingService>();
builder.Services.AddScoped<IPurchaseScreenSeedService, PurchaseScreenSeedService>();
builder.Services.AddScoped<IPurchaseReturnScreenSeedService, PurchaseReturnScreenSeedService>();

builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<IPlatformUserRepository, PlatformUserRepository>();
builder.Services.AddScoped<ITenantConnectionRepository, TenantConnectionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IPlatformPermissionResolver, PlatformPermissionResolver>();
builder.Services.AddScoped<IPlatformAuthService, PlatformAuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITenantDatabaseResolver, TenantDatabaseResolver>();
builder.Services.AddScoped<ITenantMigrationExecutor, TenantMigrationExecutor>();
builder.Services.AddScoped<ITenantMigrationService, TenantMigrationService>();
builder.Services.AddScoped<ITenantSqlFileExecutionService, TenantSqlFileExecutionService>();

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration["Cors:Origins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries)
        ?? new[] { "http://localhost:4200", "http://localhost:55762" };
    options.AddPolicy("AngularUI", policy => policy
        .WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

try
{
    await app.Services.GetRequiredService<MigrationCatalogSeeder>().EnsureRegisteredAsync();
    Log.Information("Tenant migration catalog is ready");
}
catch (Exception ex)
{
    // Keep the API available when the platform database is temporarily
    // unavailable. Migration status will report the underlying DB problem.
    Log.Error(ex, "Could not seed the built-in tenant migration catalog");
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

app.UseCors("AngularUI");

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ONE ERP Platform API v1"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("ONE ERP Platform API starting");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ONE ERP Platform API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
