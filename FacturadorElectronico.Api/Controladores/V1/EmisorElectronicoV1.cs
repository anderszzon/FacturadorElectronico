using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.CasosUso.Consultas;
using MediatR;

namespace FacturadorElectronico.Api.Controladores.V1
{
    public static class EmisorElectronicoV1
    {
        public static RouteGroupBuilder MapEmisorElectronico(this RouteGroupBuilder group)
        {
            group.MapGet("PruebaConsulta", async (
                    IMediator mediator,
                    [AsParameters] ConsultaFacturacion peticion) => await mediator.Send(peticion))
            .WithName("PruebaConsulta")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            //group.MapGet("/fe/autenticacion/api/semilla", async (
            //        IMediator mediator) => await mediator.Send(new ConsultaAutenticacionSemilla()))
            //.WithName("Semilla")
            //.Produces(StatusCodes.Status200OK)
            //.Produces(StatusCodes.Status400BadRequest);

            group.MapGet("/fe/autenticacion/api/semilla", async (IMediator mediator) =>
            {
                var respuesta = await mediator.Send(new ConsultaAutenticacionSemilla());
                if (respuesta.OperacionExitosa && !string.IsNullOrWhiteSpace(respuesta.Resultado))
                {
                    return Results.Text(respuesta.Resultado, contentType: "application/xml", statusCode: StatusCodes.Status200OK);
                }
                return Results.BadRequest(respuesta.Mensaje);
            })
            .WithName("Semilla")
            .Produces(StatusCodes.Status200OK, contentType: "application/xml")
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/fe/autenticacion/api/validacioncertificado", async (
                    IMediator mediator,
                    [AsParameters] ComandoRecepcion peticion) => await mediator.Send(peticion))
            .WithName("ValidarCertificado")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/fe/recepcion/api/ecf", async (
                    IMediator mediator,
                    [AsParameters] ComandoRecepcion peticion) => await mediator.Send(peticion))
            .WithName("Recepcion")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/fe/aprobacioncomercial/api/ecf", async (
                    IMediator mediator,
                    [AsParameters] ComandoRecepcion peticion) => await mediator.Send(peticion))
            .WithName("AprobacionComercial")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            return group;
        }
    }
}
