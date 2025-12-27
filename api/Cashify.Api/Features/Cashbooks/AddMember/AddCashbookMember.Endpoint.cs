using Cashify.Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashify.Api.Features.Cashbooks.AddMember;

public class AddCashbookMemberEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/businesses/{businessId:guid}/cashbooks/{cashbookId:guid}/members",
                [Authorize] async (Guid businessId,
                    Guid cashbookId,
                    [FromBody] AddCashbookMemberCommand command,
                    IUserContext userContext,
                    AddCashbookMemberHandler handler,
                    IValidator<AddCashbookMemberCommand> validator,
                    CancellationToken ct) =>
                {
                    var validation = await validator.ValidateAsync(command, ct);
                    if (!validation.IsValid)
                    {
                        return Results.ValidationProblem(validation.ToDictionary());
                    }

                    var actingUserId = userContext.GetUserId();
                    var ok = await handler.Handle(businessId, cashbookId, actingUserId, command, ct);
                    return ok ? Results.NoContent() : Results.Forbid();
                })
            .WithTags("Cashbooks")
            .WithDocs("Add cashbook member", "Adds a member to a cashbook; requires owner role on the business or cashbook.");
    }
}
