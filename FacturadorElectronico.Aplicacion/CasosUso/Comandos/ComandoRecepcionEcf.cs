using FacturadorElectronico.Aplicacion.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FacturadorElectronico.Aplicacion.CasosUso.Comandos
{
    public record ComandoRecepcionEcf(IFormFile Xml, string TokenBearer) : IRequest<Respuesta<string>>;
}