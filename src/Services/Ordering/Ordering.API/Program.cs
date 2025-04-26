using Ordering.API;
using Ordering.Application;
using Ordering.Infrastructure;
using Ordering.Payment.Models;
using Ordering.Payment.Services;
using Ordering.Payment.Services.Impls;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseApiServices();

//if (app.Environment.IsDevelopment())
//{
//    await app.InitialiseDatabaseAsync();
//}

app.Run();
