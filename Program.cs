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
var host = Environment.GetEnvironmentVariable("MYSQL_HOST");
var port = Environment.GetEnvironmentVariable("MYSQL_PORT");
var database = Environment.GetEnvironmentVariable("MYSQL_DATABASE");
var user = Environment.GetEnvironmentVariable("MYSQL_USER");
var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
var sslMode = Environment.GetEnvironmentVariable("MYSQL_SSL_MODE") ?? "REQUIRED";

// Ellenőrizzük és naplózzuk a környezeti változókat
Console.WriteLine($"MYSQL_HOST: {host}");
Console.WriteLine($"MYSQL_PORT: {port}");
Console.WriteLine($"MYSQL_DATABASE: {database}");
Console.WriteLine($"MYSQL_USER: {user}");
Console.WriteLine($"MYSQL_SSL_MODE: {sslMode}");

if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(database) || 
    string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
{
    throw new Exception("Missing required environment variables for database connection");
}

var connectionString = $"Server={host};" +
                      $"Port={port};" +
                      $"Database={database};" +
                      $"User={user};" +
                      $"Password={password};" +
                      $"SslMode={sslMode}";

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, 
    new MySqlServerVersion(new Version(8, 0, 13)),
     mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

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
        builder.WithOrigins("http://localhost:5173")
        // builder.WithOrigins("https://parking-garage-app.netlify.app")
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
