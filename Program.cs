using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ParkingGarageAPI.Context;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using ParkingGarageAPI.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("ParkingGarageDB"));

// Hitelesítés bekapcsolása
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new() { Title = "Parking Garage API", Version = "v1" });
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = ".AspNetCore.Cookies",
        Description = "Cookie authentication."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Adatbázis seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Seed();
}

app.Run();
