using System.Reflection;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ONEERP.ERP.API.Data;
using ONEERP.ERP.API.Middleware;
using ONEERP.ERP.API.Repositories;
using ONEERP.ERP.API.Security;
using ONEERP.ERP.API.Services;
using ONEERP.Shared.Constants;
using ONEERP.Shared.Exceptions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/erp-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers(options => options.Filters.Add<PermissionAuthorizationFilter>())
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "ONE ERP Tenant API",
        Version = "v1",
        Description = "Tenant-scoped ERP API. Use the X-Tenant-Code header to target a tenant database."
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

// Data infrastructure
builder.Services.AddSingleton<IPlatformDbConnectionFactory, PlatformDbConnectionFactory>();
builder.Services.AddScoped<ISqlHelper, SqlHelper>();
builder.Services.AddScoped<ITenantConnectionResolver, TenantConnectionResolver>();
builder.Services.AddScoped<TenantAccessor>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IApplicationSettingRepository, ApplicationSettingRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<IBusinessTypeRepository, BusinessTypeRepository>();
    builder.Services.AddScoped<IBusinessPartnerRoleRepository, BusinessPartnerRoleRepository>();
    builder.Services.AddScoped<IBusinessPartnerRepository, BusinessPartnerRepository>();
    builder.Services.AddScoped<IIndustryTypeRepository, IndustryTypeRepository>();
builder.Services.AddScoped<ICompanyGroupRepository, CompanyGroupRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IStateRepository, StateRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<ITimeZoneRepository, TimeZoneRepository>();
builder.Services.AddScoped<IGstRegistrationTypeRepository, GstRegistrationTypeRepository>();
builder.Services.AddScoped<IAddressTypeRepository, AddressTypeRepository>();
builder.Services.AddScoped<IContactTypeRepository, ContactTypeRepository>();
builder.Services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
builder.Services.AddScoped<IOrganizationTypeRepository, OrganizationTypeRepository>();
builder.Services.AddScoped<IAdministrationRepository, AdministrationRepository>();
builder.Services.AddScoped<IBranchTypeRepository, BranchTypeRepository>();
builder.Services.AddScoped<IWarehouseTypeRepository, WarehouseTypeRepository>();
builder.Services.AddScoped<IEmploymentTypeRepository, EmploymentTypeRepository>();
// Organization Repositories
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDesignationRepository, DesignationRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();



// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IAuditService, AuditService>();
    builder.Services.AddScoped<IBusinessTypeService, BusinessTypeService>();
    builder.Services.AddScoped<IBusinessPartnerRoleService, BusinessPartnerRoleService>();
    builder.Services.AddScoped<IBusinessPartnerService, BusinessPartnerService>();
    builder.Services.AddScoped<IIndustryTypeService, IndustryTypeService>();
builder.Services.AddScoped<ICompanyGroupService, CompanyGroupService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IStateService, StateService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();
builder.Services.AddScoped<IGstRegistrationTypeService, GstRegistrationTypeService>();
builder.Services.AddScoped<IAddressTypeService, AddressTypeService>();
builder.Services.AddScoped<IContactTypeService, ContactTypeService>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddScoped<IOrganizationTypeService, OrganizationTypeService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDesignationService, DesignationService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

builder.Services.AddScoped<IBranchTypeService, BranchTypeService>();
builder.Services.AddScoped<IWarehouseTypeService, WarehouseTypeService>();
builder.Services.AddScoped<IEmploymentTypeService, EmploymentTypeService>();

// Permission System Repositories (Legacy)
builder.Services.AddScoped<IPermissionModuleRepository, PermissionModuleRepository>();
builder.Services.AddScoped<IPermissionActionRepository, PermissionActionRepository>();
builder.Services.AddScoped<IModulePermissionRepository, ModulePermissionRepository>();
builder.Services.AddScoped<IFieldPermissionRepository, FieldPermissionRepository>();

// Permission System Services (Legacy)
builder.Services.AddScoped<IPermissionModuleService, PermissionModuleService>();
builder.Services.AddScoped<IPermissionActionService, PermissionActionService>();
builder.Services.AddScoped<IModulePermissionService, ModulePermissionService>();
builder.Services.AddScoped<IFieldPermissionService, FieldPermissionService>();

// Enterprise Permission Engine Repositories
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IDomainRepository, DomainRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<ISubModuleRepository, SubModuleRepository>();
builder.Services.AddScoped<IScreenRepository, ScreenRepository>();
builder.Services.AddScoped<IFieldRepository, FieldRepository>();
builder.Services.AddScoped<IActionRepository, ActionRepository>();
builder.Services.AddScoped<IRolePermissionEntryRepository, RolePermissionEntryRepository>();
builder.Services.AddScoped<IUserPermissionOverrideRepository, UserPermissionOverrideRepository>();
builder.Services.AddScoped<IRoleFieldPermissionEntryRepository, RoleFieldPermissionEntryRepository>();
builder.Services.AddScoped<IUserFieldPermissionEntryRepository, UserFieldPermissionEntryRepository>();
builder.Services.AddScoped<IDataScopeRepository, DataScopeRepository>();
builder.Services.AddScoped<IUserDataScopeOverrideRepository, UserDataScopeOverrideRepository>();
builder.Services.AddScoped<IWorkflowPermissionEntryRepository, WorkflowPermissionEntryRepository>();

// Enterprise Permission Engine Services
builder.Services.AddScoped<IWorkspaceService, WorkspaceService>();
builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ISubModuleService, SubModuleService>();
builder.Services.AddScoped<IScreenService, ScreenService>();
builder.Services.AddScoped<IFieldService, FieldService>();
builder.Services.AddScoped<IActionService, ActionService>();
builder.Services.AddScoped<IRolePermissionEntryService, RolePermissionEntryService>();
builder.Services.AddScoped<IUserPermissionOverrideService, UserPermissionOverrideService>();
builder.Services.AddScoped<IRoleFieldPermissionEntryService, RoleFieldPermissionEntryService>();
builder.Services.AddScoped<IUserFieldPermissionEntryService, UserFieldPermissionEntryService>();
builder.Services.AddScoped<IDataScopeService, DataScopeService>();
builder.Services.AddScoped<IUserDataScopeOverrideService, UserDataScopeOverrideService>();
builder.Services.AddScoped<IWorkflowPermissionEntryService, WorkflowPermissionEntryService>();

// Permission Cache & Navigation
builder.Services.AddSingleton<IPermissionCache, InMemoryPermissionCache>();
builder.Services.AddScoped<INavigationService, NavigationService>();

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

// Resolves and injects the user's permission-code claims per request
// (the API stores authoritative codes in dbo.RolePermissionsLegacy).
builder.Services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();

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

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("AngularUI");

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ONE ERP Tenant API v1"));

app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("ONE ERP Tenant API starting");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ONE ERP Tenant API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
