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
            .WithSummary("Create a new blast")
            .WithDescription(
                "Creates a new blast in the Planned state and persists a BlastCreated domain event. " +
                "The server generates the blast identifier and returns it in the response.")
            .Produces<CreateBlastResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapPost("/{blastId:guid}/holes", AddHole)
            .WithName("AddHole")
            .WithSummary("Add a hole to a blast")
            .WithDescription(
                "Adds a new hole in the Planned state to an existing blast. " +
                "When the first hole is added, the blast transitions from Planned to Loaded.")
            .Produces<AddHoleResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapPut("/{blastId:guid}/holes/{holeId:guid}/charge", ChargeHole)
            .WithName("ChargeHole")
            .WithSummary("Charge a hole")
            .WithDescription(
                "Charges a hole, transitioning it from Planned to Charged. " +
                "Rejected if the hole is already charged or ready, or if the blast has already been fired.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapPut("/{blastId:guid}/holes/{holeId:guid}/ready", MarkHoleReady)
            .WithName("MarkHoleReady")
            .WithSummary("Mark a hole as ready")
            .WithDescription(
                "Marks a charged hole as Ready. " +
                "Rejected if the hole is not in the Charged state or if the blast has already been fired.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapPost("/{blastId:guid}/fire", FireBlast)
            .WithName("FireBlast")
            .WithSummary("Fire a blast")
            .WithDescription(
                "Fires the blast when every hole is Ready, transitioning the blast to Blasted. " +
                "Rejected if any hole is not Ready, if there are no holes, or if the blast has already been fired.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        blasts.MapGet("/{blastId:guid}", GetBlast)
            .WithName("GetBlast")
            .WithSummary("Get the current blast state")
            .WithDescription(
                "Returns the current blast state from the in-memory projection, including all holes. " +
                "Does not replay the event stream.")
            .Produces<BlastResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        blasts.MapGet("/{blastId:guid}/history", GetBlastHistory)
            .WithName("GetBlastHistory")
            .WithSummary("Get the blast event history")
            .WithDescription(
                "Returns the raw ordered event stream for the blast from the event store, for audit and history purposes. " +
                "Each entry includes version, timestamp, event type, and payload.")
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
