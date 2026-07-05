using System.Text.Json.Serialization;
using BlastManagement.Application.DependencyInjection;
using BlastManagement.Infrastructure.DependencyInjection;
using BlastManagement.Api.Endpoints;
using BlastManagement.Api.ExceptionHandling;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Composition root.
//
// API design:
// - This exercise uses ASP.NET Core Minimal APIs because the HTTP surface is intentionally small.
// - Minimal APIs keep the focus on the Domain, CQRS, and Event Sourcing implementation by reducing HTTP boilerplate.
// - For a larger production application (authentication, filters, versioning, many endpoints),
//   Controllers would be an equally valid choice without impacting the Domain, Application,
//   or Infrastructure layers thanks to Clean Architecture.
//
// Event Store design:
// - One append-only event stream per Blast, keyed by BlastId.
// - Events are immutable and never updated or deleted.
// - Command handlers replay the ordered event stream, execute business rules, then append new domain events.
// - Optimistic concurrency is enforced through expectedVersion.
// - After a successful append, handlers synchronize the in-memory projection for fast reads.
// - GetBlast reads from the projection, while GetBlastHistory returns the raw event stream.
//
// AddApplication() registers CQRS handlers and the in-memory projection.
// AddInfrastructure() registers the in-memory Event Store implementation.
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
