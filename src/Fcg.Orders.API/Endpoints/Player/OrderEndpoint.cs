using Fcg.Orders.Application.Features.Commands.PlaceOrder;
using Fcg.Orders.Application.Features.Queries.GetOrderHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fcg.Orders.API.Endpoints.Player
{
    public static class OrderEndpoint
    {
        public static void MapOrderEndpoint(this WebApplication app)
        {           

            app.MapPost("/api/user/order",PlaceOrder)
                .Produces(StatusCodes.Status200OK)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .WithSummary("Realiza a compra de um game.")
                .WithDescription("Registra uma nova intenção de compra para um game do catálogo. O ID, nome e e-mail do usuário comprador são extraídos de forma segura a partir das claims do token JWT.")
                .WithName("PlaceOrder")
                .WithTags("Pedidos de jogos").RequireAuthorization("PlayersOnly"); 

            app.MapGet("/api/user/order", GetOrdersByUser)
                .Produces(StatusCodes.Status200OK)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .WithSummary("Realiza a compra de um game.")
                .WithDescription("Registra uma nova intenção de compra para um game do catálogo. O ID, nome e e-mail do usuário comprador são extraídos de forma segura a partir das claims do token JWT.")
                .WithName("PlaceOrder")
                .WithTags("Histórico de pedidos").RequireAuthorization("PlayersOnly");


        }


        private static async Task<IResult> PlaceOrder(
            [FromServices] ISender sender,
            [FromBody] IEnumerable<Guid> gamesIds,
            CancellationToken cancellationToken,
            ClaimsPrincipal user)
        {
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Results.Unauthorized();
            }

            var userId = Guid.Parse(currentUserIdClaim);

            var orderCommand = new PlaceOrderCommand(userId, gamesIds);

            var response = await sender.Send(orderCommand);

            if (!response)
                return Results.BadRequest();

            return Results.Ok();
        }

        private static async Task<IResult> GetOrdersByUser(
            [FromServices] ISender sender,
            CancellationToken cancellationToken,
            ClaimsPrincipal user)
        {
            var currentUserIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserIdClaim))
            {
                return Results.Unauthorized();
            }

            var userId = Guid.Parse(currentUserIdClaim);

            var query = new GetOrderHistoryQuery(userId);
            var response = await sender.Send(query, cancellationToken);

            return Results.Ok(response);
        }
    }
}
