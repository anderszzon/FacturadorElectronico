using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;
using System.Xml.Linq;

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

        try
        {
            // 3. Leer el XML entrante
            string xmlEntrante;
            using (var reader = new StreamReader(request.Xml.OpenReadStream()))
            {
                xmlEntrante = await reader.ReadToEndAsync(cancellationToken);
            }

            // 4. Extraer campos del XML entrante
            XDocument doc = XDocument.Parse(xmlEntrante);
            XNamespace ns = doc.Root?.Name.Namespace ?? XNamespace.None;

            string rncEmisor = doc.Descendants(ns + "RNCEmisor").FirstOrDefault()?.Value ?? "";
            string rncComprador = doc.Descendants(ns + "RNCComprador").FirstOrDefault()?.Value ?? "";
            string eNCF = doc.Descendants(ns + "eNCF").FirstOrDefault()?.Value ?? "";

            string fechaHoraActual = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

            // 5. Armar el XML del Acuse de Recibo con las declaraciones XML Schema Instance (xsi)
            string xmlSinFirmar = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                                    <ARECF xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                                        <DetalleAcusedeRecibo>
                                            <Version>1.0</Version>
                                            <RNCEmisor>{rncEmisor}</RNCEmisor>
                                            <RNCComprador>{rncComprador}</RNCComprador>
                                            <eNCF>{eNCF}</eNCF>
                                            <Estado>0</Estado>
                                            <FechaHoraAcuseRecibo>{fechaHoraActual}</FechaHoraAcuseRecibo>
                                        </DetalleAcusedeRecibo>
                                    </ARECF>";

            // 6. Firmar el XML del Acuse de Recibo
            string xmlFirmado = _autenticacion.FirmarAcuseRecibo(xmlSinFirmar);

            // 7. Devolver el XML firmado
            return Respuesta<string>.Ok(xmlFirmado);
        }
        catch (Exception ex)
        {
            return Respuesta<string>.Fail($"Error procesando acuse: {ex.Message}");
        }
    }
}