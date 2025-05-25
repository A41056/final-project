using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using User.API.Models;
using User.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings:SMTPEmailSetting")
);

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database"));
    options.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;
    options.Schema.For<User.API.Models.User>();
});

if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<UserInitialData>();

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

builder.Services.AddScoped<GoogleLoginService>();
builder.Services.AddScoped<FacebookLoginService>();
builder.Services.AddSingleton<SocialLoginFactory>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAuthorization();
builder.Services.AddCarter();

var app = builder.Build();

app.UseCors("AllowAll");

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");
    var headers = context.Request.Headers["Authorization"];
    Console.WriteLine($"Authorization Header: {headers}");
    await next(context);
    Console.WriteLine($"Response: {context.Response.StatusCode}");
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

app.Run();