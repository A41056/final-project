﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            ValidIssuer = issuer,   // "Quanggiap299"
            ValidAudience = audience, // "PhamQuangGiapA41056"
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
        policy.WithOrigins("*") // Chỉ cho phép origin này
              .AllowAnyMethod() // Cho phép tất cả HTTP methods (GET, POST, PUT, v.v.)
              .AllowAnyHeader(); // Cho phép tất cả headers
    });
});

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

app.MapCarter();

app.UseAuthentication();
app.UseAuthorization();

app.Run();