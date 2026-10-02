using FacturadorElectronico.Aplicacion.Envoltorios;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FacturadorElectronico.Aplicacion.CasosUso.Comandos;

public record ComandoValidarCertificado(IFormFile Xml) : IRequest<Respuesta<RespuestaTokenDto>>;

public class RespuestaTokenDto
{
    public string Token { get; set; } = string.Empty;
    public string Expira { get; set; } = string.Empty;
    public string Expedido { get; set; } = string.Empty;
}
