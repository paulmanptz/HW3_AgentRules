using MasterApp.Application;
using MasterApp.Infrastructure;
using MasterApp.Auth.Application.Interfaces;
using MasterApp.Auth.Application.Services;
using MasterApp.Auth.Infrastructure.Data;
using MasterApp.Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Files Layer
MasterApp.Files.Contracts.Implementations.FileContractsModule.AddFilesContracts(builder.Services);
MasterApp.Files.UseCases.FilesUseCasesModule.AddFilesUseCases(builder.Services);
MasterApp.Files.Infrastructure.FilesInfrastructureModule.AddFilesInfrastructure(builder.Services, builder.Configuration);
MasterApp.Files.Controllers.FilesControllersModule.AddFilesControllers(builder.Services);

// Add Auth Layer
var authDataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
authDataSourceBuilder.MapEnum<MasterApp.Auth.Domain.Entities.RoleType>("Auth.RoleType");
var authDataSource = authDataSourceBuilder.Build();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(authDataSource, o => o.MapEnum<MasterApp.Auth.Domain.Entities.RoleType>("RoleType", "Auth")));
builder.Services.AddScoped<IAuthDbContext>(provider => provider.GetRequiredService<AuthDbContext>());

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDispatcherAuthService, DispatcherAuthService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<ISsoTokenService, SsoTokenService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger with JWT Auth
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MasterApp WebAPI", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Auth
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "super_secret_key_which_is_very_long_for_hmac_sha256_at_least_32_bytes";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MasterApp",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MasterAppMobile",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
