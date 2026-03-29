using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using NotesApp.Api.Common;
using NotesApp.Api.Middleware;
using NotesApp.Data;
using NotesApp.Data.Repositories;
using NotesApp.Services;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JwtSettings:Key is missing.");
var jwtIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is missing.");
var jwtAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is missing.");
var jwtDuration = jwtSettings["DurationInMinutes"] ?? throw new InvalidOperationException("JwtSettings:DurationInMinutes is missing.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("JwtSettings:Key must be at least 32 bytes.");

if (!double.TryParse(jwtDuration, out var parsedDuration) || parsedDuration <= 0)
    throw new InvalidOperationException("JwtSettings:DurationInMinutes must be a positive number.");

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid request." : e.ErrorMessage)
                .Distinct()
                .ToArray();

            var response = ApiResponse.Failure(
                "Validation failed.",
                errors,
                context.HttpContext.TraceIdentifier);

            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DI
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = ApiResponse.Failure(
                    "Authentication is required.",
                    null,
                    context.HttpContext.TraceIdentifier);

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = ApiResponse.Failure(
                    "You do not have permission to access this resource.",
                    null,
                    context.HttpContext.TraceIdentifier);

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        };
    });

// Configure CORS for Vue frontend (local + hosted frontend origins)
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
var corsOrigins = (configuredOrigins is { Length: > 0 } ? configuredOrigins : Array.Empty<string>())
    .SelectMany(origin => origin.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();

if (corsOrigins.Count == 0)
{
    corsOrigins.Add("http://localhost:5173");
    corsOrigins.Add("http://127.0.0.1:5173");
    corsOrigins.Add("https://alchemyyyy.github.io");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

// On Render, TLS is terminated at the edge proxy. Redirect only in local/dev host.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
