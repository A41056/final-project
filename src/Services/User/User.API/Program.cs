using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Database"));
    options.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;
    options.Schema.For<User.API.Models.User>();
});

if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<UserInitialData>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost5173", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174") // Chỉ cho phép origin này
              .AllowAnyMethod() // Cho phép tất cả HTTP methods (GET, POST, PUT, v.v.)
              .AllowAnyHeader() // Cho phép tất cả headers
              .AllowCredentials(); // Nếu cần gửi cookie hoặc auth credentials
    });
});

builder.Services.AddAuthorization();
builder.Services.AddCarter();

var app = builder.Build();

app.UseCors("AllowLocalhost5173");
app.UseAuthentication();
app.UseAuthorization();
app.MapCarter();

app.Run();