using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;

public class ManejadorRecepcionEcf(IAutenticacion autenticacion)
    : IRequestHandler<ComandoRecepcionEcf, Respuesta<string>>
{
    private readonly IAutenticacion _autenticacion = autenticacion;

    public async Task<Respuesta<string>> Handle(ComandoRecepcionEcf request, CancellationToken cancellationToken)
    {
        // 1. Validar presencia del archivo XML
        if (request.Xml == null || request.Xml.Length == 0)
        {
            return Respuesta<string>.Fail("El archivo XML del e-CF es requerido.");
        }

        // 2. Validar presencia del Token Bearer en la petición
        if (string.IsNullOrWhiteSpace(request.TokenBearer))
        {
            return Respuesta<string>.Fail("El token de autorización (Bearer) es requerido.");
        }

        string urlRecepcion = "https://ecf.dgii.gov.do/testecf/recepcion/api/ecf";

        try
        {
            string xmlContenido;
            using (var streamReader = new StreamReader(request.Xml.OpenReadStream()))
            {
                xmlContenido = await streamReader.ReadToEndAsync();
            }

            // Tomar el nombre del archivo recibido o usar uno por defecto si viene vacío
            string nombreArchivoXml = !string.IsNullOrWhiteSpace(request.Xml.FileName)
                ? request.Xml.FileName
                : "ecf.xml";

            string xmlRespuestaAcuse = await _autenticacion.ProcesarRecepcionEcfXmlAsync(
                urlRecepcion,
                xmlContenido,
                request.TokenBearer,
                nombreArchivoXml,
                cancellationToken);

            if (string.IsNullOrWhiteSpace(xmlRespuestaAcuse))
            {
                return Respuesta<string>.Fail("No se obtuvo respuesta de acuse por parte de la DGII.");
            }

            return Respuesta<string>.Ok(xmlRespuestaAcuse);
        }
        catch (Exception ex)
        {
            return Respuesta<string>.Fail($"Error en la recepción de e-CF: {ex.Message}");
        }
    }
}