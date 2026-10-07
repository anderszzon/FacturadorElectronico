using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.CasosUso.Consultas;
using MediatR;
using System.Text;

namespace FacturadorElectronico.Api.Controladores.V1
{
    public static class EmisorElectronicoV1
    {
        public static RouteGroupBuilder MapEmisorElectronico(this RouteGroupBuilder group)
        {
            //group.MapGet("PruebaConsulta", async (
            //        IMediator mediator,
            //        [AsParameters] ConsultaFacturacion peticion) => await mediator.Send(peticion))
            //.WithName("PruebaConsulta")
            //.Produces(StatusCodes.Status200OK)
            //.Produces(StatusCodes.Status400BadRequest);

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

            //group.MapPost("/fe/autenticacion/api/validacioncertificado", async (
            //        IMediator mediator,
            //        [AsParameters] ComandoRecepcion peticion) => await mediator.Send(peticion))
            //.WithName("ValidarCertificado")
            //.Produces(StatusCodes.Status200OK)
            //.Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/fe/autenticacion/api/validacioncertificado", async (
                IFormFile xml,
                IMediator mediator) =>
            {
                var comando = new ComandoValidarCertificado(xml);
                var respuesta = await mediator.Send(comando);

                if (respuesta.OperacionExitosa && respuesta.Resultado != null)
                {
                    return Results.Ok(respuesta.Resultado);
                }

                return Results.BadRequest(respuesta.Mensaje);
            })
            .WithName("ValidarCertificado")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<RespuestaTokenDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();

            //group.MapPost("/fe/recepcion/api/ecf", async (
            //        IMediator mediator,
            //        [AsParameters] ComandoRecepcion peticion) => await mediator.Send(peticion))
            //.WithName("Recepcion")
            //.Produces(StatusCodes.Status200OK)
            //.Produces(StatusCodes.Status400BadRequest);

            group.MapPost("/fe/recepcion/api/ecf", async (
                HttpContext httpContext,
                IMediator mediator) =>
            {
                // 1. Obtener el archivo multipart sin importar el nombre del campo del Form
                var file = httpContext.Request.Form.Files.FirstOrDefault();
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest("No se recibió ningún archivo XML en la petición.");
                }

                // 2. Extraer el token Bearer del header 'Authorization'
                string tokenBearer = httpContext.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrWhiteSpace(tokenBearer))
                {
                    tokenBearer = httpContext.Request.Headers["authorization"].ToString();
                }

                // 3. Procesar y firmar el Acuse de Recibo
                var comando = new ComandoRecepcionEcf(file, tokenBearer);
                var respuesta = await mediator.Send(comando);

                if (respuesta.OperacionExitosa && !string.IsNullOrWhiteSpace(respuesta.Resultado))
                {
                    // 4. Crear codificación UTF-8 pura SIN BOM (false)
                    var utf8WithoutBom = new UTF8Encoding(false);
                    byte[] xmlBytes = utf8WithoutBom.GetBytes(respuesta.Resultado);

                    // 5. Retornar directamente los bytes garantizando cero caracteres de control
                    return Results.Bytes(
                        contents: xmlBytes,
                        contentType: "text/xml; charset=utf-8"                    );
                }

                return Results.BadRequest(respuesta.Mensaje);
            })
            .WithName("Recepcion")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(StatusCodes.Status200OK, contentType: "text/xml")
            .Produces(StatusCodes.Status400BadRequest)
            .DisableAntiforgery();

            //group.MapPost("/fe/aprobacioncomercial/api/ecf", async (
            //        IMediator mediator,
            //        [AsParameters] ComandoRecepcion peticion) => await mediator.Send(peticion))
            //.WithName("AprobacionComercial")
            //.Produces(StatusCodes.Status200OK)
            //.Produces(StatusCodes.Status400BadRequest);



            //group.MapPost("/fe/aprobacioncomercial/api/ecf", async (
            //        IMediator mediator,
            //        [AsParameters] ComandoRecepcion peticion) => await mediator.Send(peticion))
            //.WithName("AprobacionComercial")
            //.Produces(StatusCodes.Status200OK)
            //.Produces(StatusCodes.Status400BadRequest);

            return group;
        }
    }
}
