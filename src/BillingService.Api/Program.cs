using System.Text;
using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using BillingService.Application.Mapping;
using BillingService.Application.Services;
using BillingService.Application.Validators;
using BillingService.Infrastructure.Identity;
using BillingService.Infrastructure.Persistence;
using BillingService.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog ----
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.File("logs/billing-service-.log", rollingInterval: RollingInterval.Day);
});

// ---- QuestPDF license (community tier - free for small revenue orgs) ----
QuestPDF.Settings.License = LicenseType.Community;

// ---- Database ----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ---- Identity ----
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<BillingDbContext>()
    .AddDefaultTokenProviders();

// ---- JWT auth ----
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ---- AutoMapper ----
// No license key needed for individual/small-project use - AutoMapper only
// logs a message about it, nothing is disabled. Silence that log line since
// it's just noise for this project size (see the license-config docs if you
// ever need a real key: https://docs.automapper.io/en/latest/License-configuration.html)
builder.Logging.AddFilter("LuckyPennySoftware.AutoMapper.License", LogLevel.None);
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

// ---- Validation ----
builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
builder.Services.AddScoped<IValidator<RecordPaymentRequest>, RecordPaymentRequestValidator>();

// ---- Repositories ----
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ---- Application services ----
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// ---- API plumbing ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    // Tighten this to your actual Angular dev/prod origins before shipping.
    options.AddPolicy("AngularClient", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AngularClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Simple liveness check until the real controllers land in phase 3.
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "billing-service-api" }));

app.Run();
