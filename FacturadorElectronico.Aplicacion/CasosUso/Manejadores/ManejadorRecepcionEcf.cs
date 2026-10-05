using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;
using System.Globalization;
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

            // Búsqueda flexible por LocalName por si el XML e-CF entrante incluye o no Namespace
            string rncEmisor = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "RNCEmisor")?.Value ?? "";
            string rncComprador = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "RNCComprador")?.Value ?? "";
            string eNCF = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "eNCF")?.Value ?? "";

            // Formato de fecha estricto dd-MM-yyyy HH:mm:ss según XSD de la DGII
            string fechaHoraActual = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            XNamespace ns = "http://www.dgii.gov.do/ecf/v1.0";
            XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

            var docArecf = new XDocument(
                        new XDeclaration("1.0", "utf-8", "no"),
                        new XElement(ns + "ARECF",
                            new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                            new XElement(ns + "DetalleAcusedeRecibo",
                                new XElement(ns + "Version", "1.0"),
                                new XElement(ns + "RNCEmisor", rncEmisor),
                                new XElement(ns + "RNCComprador", rncComprador),
                                new XElement(ns + "eNCF", eNCF),
                                new XElement(ns + "Estado", "0"),
                                new XElement(ns + "FechaHoraAcuseRecibo", fechaHoraActual)
                            )
                        )
                    );

            string xmlSinFirmar = docArecf.ToString(SaveOptions.DisableFormatting);

            string xmlFirmado = _autenticacion.FirmarAcuseRecibo(xmlSinFirmar);

            return Respuesta<string>.Ok(xmlFirmado);
        }
        catch (Exception ex)
        {
            return Respuesta<string>.Fail($"Error procesando acuse: {ex.Message}");
        }
    }
}