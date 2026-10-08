using FacturadorElectronico.Aplicacion.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FacturadorElectronico.Aplicacion.CasosUso.Comandos
{
    public record ComandoFirmarDeclaracionJurada(IFormFile Xml) : IRequest<Respuesta<string>>;
}