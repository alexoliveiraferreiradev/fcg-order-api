using Fcg.Orders.API.Extentions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddServicesExtensions()
        .AddSwaggerService();

var app = builder.Build();
app.AddAppConfiguration();
app.Run();  