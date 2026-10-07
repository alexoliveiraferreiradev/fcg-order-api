using Fcg.Orders.API.Extentions;
using Fcg.Orders.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServicesExtensions();

var app = builder.Build();
app.AddAppConfiguration();
await app.Services.EnsureMongoIndexesAsync();
app.Run();  