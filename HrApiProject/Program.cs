using HrApiProject.Data;
using HrApiProject.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Registrando el DBContext con Oracle
var connectionStrings = builder.Configuration.GetConnectionString("OracleDB");
builder.Services.AddDbContext<HrDbContext>(options => options.UseOracle(connectionStrings));

// Add services to the container.
builder.Services.AddControllers();

// Herramienta nativa de Microsoft para OpenAPI (.NET 9+)
builder.Services.AddOpenApi();

// Configuración del servicio de Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // cada IP tiene su propio límite.
    options.AddPolicy("LoginPolicy", context =>
    {
        var ip = context.Connection.RemoteIpAddress?
            .ToString() ?? "unknown";

        var key = builder.Environment.IsDevelopment()
        ? $"login:{context.Connection.RemoteIpAddress}"
        : "login:demo-shared";


        return RateLimitPartition.GetFixedWindowLimiter(partitionKey: key, factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });


    });

    // cada usuario autenticado tiene su propio límite.
    options.AddPolicy("GeneralPolicy", context =>
    {
        var userId = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? context.User.Identity?.Name;

        var ip = context.Connection.RemoteIpAddress?
            .ToString() ?? "unknown";

        var key = context.User.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(userId)
                ? $"user:{userId}"
                : $"ip:{ip}";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: key,
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 60,
                TokensPerPeriod = 10,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        var response = context.HttpContext.Response;

        response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        int? retryAfterSeconds = null;

        if (context.Lease.TryGetMetadata(
            MetadataName.RetryAfter, out var retryAfter))
        {
            retryAfterSeconds = (int)Math.Clamp(
                Math.Ceiling(retryAfter.TotalSeconds),
                1,
                int.MaxValue);

            response.Headers["Retry-After"] =
                retryAfterSeconds.Value.ToString(
                    CultureInfo.InvariantCulture);
        }

        response.Headers["Cache-Control"] = "no-store";

        await response.WriteAsJsonAsync(new
        {
            message = "Has realizado demasiadas solicitudes. "
                + "Espera unos momentos e inténtalo nuevamente.",
            retryAfterSeconds
        }, cancellationToken: cancellationToken);
    };
});

// configuracion jwt

// leer la configuración de appsetiongs
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("SecretKey")!;

// configurar el servicio de autenticación
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(optiones =>
{

    optiones.TokenValidationParameters = new TokenValidationParameters
    {

        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey  = true,
        ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
        ValidAudience = jwtSettings.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        
    };

});

// Registrar la capa de servicios en el contenedor de dependencias


builder.Services.AddScoped<IRegionService, RegionService>();

builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddScoped<ILocationService, LocationService>();

builder.Services.AddScoped<IEmployeeService, EmployeeServices>();

builder.Services.AddScoped<IJobHistoryService, JobHistoryServices>();

builder.Services.AddScoped<ICountryService, CountryService>();

builder.Services.AddScoped<IJobService, JobServices>();

// cors

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontEnd", policy =>
    {
        policy.WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});


//builder.Services
//    .AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.DefaultIgnoreCondition =
//            JsonIgnoreCondition.WhenWritingNull;
//    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    var proxies = builder.Configuration
        .GetSection("ReverseProxy:KnownProxies")
        .Get<string[]>() ?? [];

    foreach (var proxy in proxies)
    {
        options.KnownProxies.Add(IPAddress.Parse(proxy));
    }
});

var app = builder.Build();

app.UseMiddleware<HrApiProject.Middlewares.GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "HrApi v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();   

app.UseCors("ReactFrontEnd");

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();

app.Run();
