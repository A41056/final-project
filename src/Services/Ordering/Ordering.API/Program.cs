using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ordering.API;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Payment.Models;
using Ordering.Payment.Services;
using Ordering.Payment.Services.Impls;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            )
        };
        options.RequireHttpsMetadata = false;
    });

// Add services to the container.
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IPaymentFactory, PaymentFactory>();
builder.Services.Configure<BillingSettingOptions>(builder.Configuration.GetSection(BillingSettingOptions.BillingSetting));
builder.Services.AddHttpClient("CatalogService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CATALOG_API_URL"] ?? "https://localhost:6069");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddHttpClient("OrderingService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ORDER_API_URL"] ?? "http://localhost:6003");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
app.UseApiServices();

//if (app.Environment.IsDevelopment())
//{
//    await app.InitialiseDatabaseAsync();
//}
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
