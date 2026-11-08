using HrApiProject.Data;
using HrApiProject.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Oracle.EntityFrameworkCore.Internal;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Registrando el DBContext con Oracle
var connectionStrings = builder.Configuration.GetConnectionString("OracleDB");
builder.Services.AddDbContext<HrDbContext>(options => options.UseOracle(connectionStrings));

// Add services to the container.
builder.Services.AddControllers();

// Herramienta nativa de Microsoft para OpenAPI (.NET 9+)
builder.Services.AddOpenApi();

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




//builder.Services
//    .AddControllers()
//    .AddJsonOptions(options =>
//    {
//        options.JsonSerializerOptions.DefaultIgnoreCondition =
//            JsonIgnoreCondition.WhenWritingNull;
//    });


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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
