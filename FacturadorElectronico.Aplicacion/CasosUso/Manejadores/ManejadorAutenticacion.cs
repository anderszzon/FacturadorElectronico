using FacturadorElectronico.Aplicacion.CasosUso.Consultas;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;

namespace FacturadorElectronico.Aplicacion.CasosUso.Manejadores;
public class ManejadorAutenticacion(
    IAutenticacion autenticacion) : IRequestHandler<ConsultaAutenticacionSemilla, Respuesta<string>>
{
    private readonly IAutenticacion _autenticacion = autenticacion;

    public async Task<Respuesta<string>> Handle(ConsultaAutenticacionSemilla request, CancellationToken cancellationToken)
    {
        string urlSemilla = "https://ecf.dgii.gov.do/testecf/autenticacion/api/Autenticacion/Semilla";

        try
        {
            string xmlSemilla = await _autenticacion.ObtenerSemillaXmlAsync(urlSemilla, cancellationToken);

            if (string.IsNullOrWhiteSpace(xmlSemilla))
            {
                return Respuesta<string>.Fail("No se pudo obtener la semilla de la DGII (Respuesta vacía).");
            }

            return Respuesta<string>.Ok(xmlSemilla);
        }
        catch (Exception ex)
        {
            return Respuesta<string>.Fail($"Error en la petición de semilla: {ex.Message}");
        }
    }
}

