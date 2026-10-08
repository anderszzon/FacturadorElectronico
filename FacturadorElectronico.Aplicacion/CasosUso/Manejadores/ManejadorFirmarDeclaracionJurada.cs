using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;

namespace FacturadorElectronico.Aplicacion.CasosUso.Manejadores;

public class ManejadorFirmarDeclaracionJurada(IAutenticacion autenticacion)
    : IRequestHandler<ComandoFirmarDeclaracionJurada, Respuesta<string>>
{
    private readonly IAutenticacion _autenticacion = autenticacion;

    public async Task<Respuesta<string>> Handle(ComandoFirmarDeclaracionJurada request, CancellationToken cancellationToken)
    {
        if (request.Xml == null || request.Xml.Length == 0)
        {
            return Respuesta<string>.Fail("El archivo XML de la Declaración Jurada es requerido.");
        }

        try
        {
            string xmlEntrante;
            using (var reader = new StreamReader(request.Xml.OpenReadStream()))
            {
                xmlEntrante = await reader.ReadToEndAsync(cancellationToken);
            }

            // Reutiliza la misma lógica de firma XMLDSig RSA-SHA256
            string xmlFirmado = _autenticacion.FirmarAcuseRecibo(xmlEntrante);

            return Respuesta<string>.Ok(xmlFirmado);
        }
        catch (Exception ex)
        {
            return Respuesta<string>.Fail($"Error al firmar la Declaración Jurada: {ex.Message}");
        }
    }
}