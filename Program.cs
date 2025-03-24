using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ParkingGarageAPI.Context;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using ParkingGarageAPI.Auth;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;

// Környezeti változók betöltése a .env fájlból
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// MySQL kapcsolat
var connectionString = $"server={Environment.GetEnvironmentVariable("MYSQL_HOST")};" +
                    $"port={Environment.GetEnvironmentVariable("MYSQL_PORT")};" +
                    $"database={Environment.GetEnvironmentVariable("MYSQL_DATABASE")};" +
                    $"user={Environment.GetEnvironmentVariable("MYSQL_USER")};" +
                    $"password={Environment.GetEnvironmentVariable("MYSQL_PASSWORD")};" +
                    $"SslMode={Environment.GetEnvironmentVariable("MYSQL_SSL_MODE") ?? "REQUIRED"}";

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 13))));

// Hitelesítés
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/api/users/login";
        options.LogoutPath = "/api/users/logout";
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new AdminRequirement()));
});

// Admin jogosultság kezelő regisztrálása
builder.Services.AddScoped<IAuthorizationHandler, AdminAuthorizationHandler>();

// Email és számlázási szolgáltatások regisztrálása
builder.Services.AddScoped<ParkingGarageAPI.Services.IEmailService, ParkingGarageAPI.Services.EmailService>();
builder.Services.AddScoped<ParkingGarageAPI.Services.IInvoiceService, ParkingGarageAPI.Services.InvoiceService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Swagger mindig elérhető
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5175")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Swagger mindig elérhető
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParkingGarage API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Adatbázis seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Seed();
}

// Lokális fejlesztéshez port beállítása
app.Urls.Clear();
app.Urls.Add("http://localhost:5025");

app.Run();
