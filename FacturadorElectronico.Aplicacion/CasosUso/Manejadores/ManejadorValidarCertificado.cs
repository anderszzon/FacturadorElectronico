using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;
using System.Text.Json;

namespace FacturadorElectronico.Aplicacion.CasosUso.Manejadores;

public class ManejadorValidarCertificado(
    IAutenticacion autenticacion) : IRequestHandler<ComandoValidarCertificado, Respuesta<RespuestaTokenDto>>
{
    private readonly IAutenticacion _autenticacion = autenticacion;

    public async Task<Respuesta<RespuestaTokenDto>> Handle(ComandoValidarCertificado request, CancellationToken cancellationToken)
    {
        if (request.Xml == null || request.Xml.Length == 0)
        {
            return Respuesta<RespuestaTokenDto>.Fail("El archivo XML de la semilla firmada es requerido.");
        }

        string urlValidacion = "https://ecf.dgii.gov.do/testecf/autenticacion/api/Autenticacion/ValidarSemilla";

        try
        {
            string xmlContenido;
            using (var streamReader = new StreamReader(request.Xml.OpenReadStream()))
            {
                xmlContenido = await streamReader.ReadToEndAsync();
            }

            string respuestaJson = await _autenticacion.ValidarCertificadoXmlAsync(urlValidacion, xmlContenido, cancellationToken);

            if (string.IsNullOrWhiteSpace(respuestaJson))
            {
                return Respuesta<RespuestaTokenDto>.Fail("No se obtuvo respuesta de validación por parte de la DGII.");
            }

            var tokenDto = JsonSerializer.Deserialize<RespuestaTokenDto>(respuestaJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenDto == null)
            {
                return Respuesta<RespuestaTokenDto>.Fail("No se pudo deserializar el token emitido por la DGII.");
            }

            return Respuesta<RespuestaTokenDto>.Ok(tokenDto);
        }
        catch (Exception ex)
        {
            return Respuesta<RespuestaTokenDto>.Fail($"Error en la validación del certificado: {ex.Message}");
        }
    }
}