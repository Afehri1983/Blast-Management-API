using BlastManagement.Application.CommandHandlers;
using BlastManagement.Application.Commands;
using BlastManagement.Application.Queries;
using BlastManagement.Application.QueryHandlers;
using BlastManagement.Api.Contracts.Requests;
using BlastManagement.Api.Contracts.Responses;
using BlastManagement.Api.Mapping;
using BlastManagement.Domain.ValueObjects;

namespace BlastManagement.Api.Endpoints;

internal static class BlastEndpoints
{
    public static void MapBlastEndpoints(this WebApplication app)
    {
        var blasts = app.MapGroup("/blasts");

        blasts.MapPost("/", CreateBlast)
            .WithName("CreateBlast")
            .Produces<CreateBlastResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapPost("/{blastId:guid}/holes", AddHole)
            .WithName("AddHole")
            .Produces<AddHoleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapPut("/{blastId:guid}/holes/{holeId:guid}/charge", ChargeHole)
            .WithName("ChargeHole")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapPut("/{blastId:guid}/holes/{holeId:guid}/ready", MarkHoleReady)
            .WithName("MarkHoleReady")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapPost("/{blastId:guid}/fire", FireBlast)
            .WithName("FireBlast")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapGet("/{blastId:guid}", GetBlast)
            .WithName("GetBlast")
            .Produces<BlastResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        blasts.MapGet("/{blastId:guid}/history", GetBlastHistory)
            .WithName("GetBlastHistory")
            .Produces<IReadOnlyList<EventEnvelopeResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateBlast(
        CreateBlastRequest request,
        CreateBlastCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var blastId = await handler.HandleAsync(new CreateBlastCommand(request.Name), cancellationToken);
        return Results.Created($"/blasts/{blastId}", new CreateBlastResponse(blastId));
    }

    private static async Task<IResult> AddHole(
        Guid blastId,
        AddHoleRequest request,
        AddHoleCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AddHoleCommand(
                blastId,
                request.HoleId,
                request.Name,
                new Position(request.PositionX, request.PositionY, request.PositionZ),
                request.Direction,
                request.Inclination),
            cancellationToken);

        return Results.Created(
            $"/blasts/{blastId}/holes/{request.HoleId}",
            new AddHoleResponse(request.HoleId));
    }

    private static async Task<IResult> ChargeHole(
        Guid blastId,
        Guid holeId,
        ChargeHoleCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ChargeHoleCommand(blastId, holeId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> MarkHoleReady(
        Guid blastId,
        Guid holeId,
        MarkHoleReadyCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new MarkHoleReadyCommand(blastId, holeId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> FireBlast(
        Guid blastId,
        FireBlastCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new FireBlastCommand(blastId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetBlast(
        Guid blastId,
        GetBlastQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var blast = await handler.HandleAsync(new GetBlastQuery(blastId), cancellationToken);
        return Results.Ok(ApiMappers.ToResponse(blast));
    }

    private static async Task<IResult> GetBlastHistory(
        Guid blastId,
        GetBlastHistoryQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var history = await handler.HandleAsync(new GetBlastHistoryQuery(blastId), cancellationToken);
        return Results.Ok(history.Select(ApiMappers.ToResponse).ToList());
    }
}
