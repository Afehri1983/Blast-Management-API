using System.Text.Json.Serialization;
using BlastManagement.Application.DependencyInjection;
using BlastManagement.Infrastructure.DependencyInjection;
using BlastManagement.Api.Endpoints;
using BlastManagement.Api.ExceptionHandling;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Composition root: Application registers handlers/projection; Infrastructure registers in-memory event store.
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddExceptionHandler<ApplicationExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blast Management API",
        Version = "v1"
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapBlastEndpoints();

app.Run();
