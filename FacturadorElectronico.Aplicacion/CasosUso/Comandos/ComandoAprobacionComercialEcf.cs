using FacturadorElectronico.Aplicacion.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FacturadorElectronico.Aplicacion.CasosUso.Comandos
{
    public record ComandoAprobacionComercialEcf(IFormFile Xml, string TokenBearer) : IRequest<Respuesta<string>>;
}