using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Xml.Linq;

namespace FacturadorElectronico.Aplicacion.CasosUso.Manejadores;
public class ManejadorAprobacionComercialEcf(IAutenticacion autenticacion)
    : IRequestHandler<ComandoAprobacionComercialEcf, Respuesta<string>>
{
    private readonly IAutenticacion _autenticacion = autenticacion;

    private const string CadenaConexion = "Server=tcp:mtpprod.database.windows.net,1433;Initial Catalog=MTPDatabase;Persist Security Info=False;User ID=MTPadmin;Password=Pass@word1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

    public async Task<Respuesta<string>> Handle(ComandoAprobacionComercialEcf request, CancellationToken cancellationToken)
    {
        // 1. Validar presencia del archivo XML
        if (request.Xml == null || request.Xml.Length == 0)
        {
            return Respuesta<string>.Fail("El archivo XML de aprobación comercial es requerido.");
        }

        // 2. Validar presencia del Token Bearer
        if (string.IsNullOrWhiteSpace(request.TokenBearer))
        {
            return Respuesta<string>.Fail("El token de autorización (Bearer) es requerido.");
        }

        try
        {
            // 3. Leer el XML entrante (e-CF o datos de aprobación)
            string xmlEntrante;
            using (var reader = new StreamReader(request.Xml.OpenReadStream()))
            {
                xmlEntrante = await reader.ReadToEndAsync(cancellationToken);
            }

            // 4. Extraer campos requeridos del e-CF entrante
            XDocument doc = XDocument.Parse(xmlEntrante);

            string rncEmisor = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "RNCEmisor")?.Value ?? "";
            string rncComprador = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "RNCComprador")?.Value ?? "";
            string eNCF = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "eNCF")?.Value ?? "";

            // Extracción y formateo de la fecha de emisión a formato dd-MM-yyyy
            string fechaEmisionRaw = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "FechaEmision")?.Value ?? DateTime.Now.ToString("yyyy-MM-dd");
            string fechaEmision = FormatearFechaEmision(fechaEmisionRaw);

            // Extracción y formateo de MontoTotal con 2 decimales estricto
            string montoTotalRaw = doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "MontoTotal")?.Value ?? "0.00";
            string montoTotal = FormatearMontoTotal(montoTotalRaw);

            string fechaHoraActual = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            // 5. Construir el XML de Aprobación Comercial (ACECF) siguiendo estrictamente el XSD
            var detalleElement = new XElement("DetalleAprobacionComercial",
                new XElement("Version", "1.0"),
                new XElement("RNCEmisor", rncEmisor),
                new XElement("eNCF", eNCF),
                new XElement("FechaEmision", fechaEmision),
                new XElement("MontoTotal", montoTotal),
                new XElement("RNCComprador", rncComprador),
                new XElement("Estado", "1"), // 1 = e-CF Aceptado, 2 = e-CF Rechazado
                new XElement("FechaHoraAprobacionComercial", fechaHoraActual)
            );

            var docAcecf = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("ACECF",
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    detalleElement
                )
            );

            // Deshabilitar formateo de espacios/saltos para asegurar compatibilidad con la firma C14N
            string xmlSinFirmar = docAcecf.ToString(SaveOptions.DisableFormatting);

            // 6. Firmar el XML de Aprobación Comercial
            string xmlFirmado = _autenticacion.FirmarAcuseRecibo(xmlSinFirmar);

            // 7. Persistir la telemetría en base de datos
            await GuardarEnBaseDeDatosAsync(rncEmisor, rncComprador, eNCF, xmlEntrante, xmlFirmado, cancellationToken);

            return Respuesta<string>.Ok(xmlFirmado);
        }
        catch (Exception ex)
        {
            return Respuesta<string>.Fail($"Error procesando aprobación comercial: {ex.Message}");
        }
    }

    private static string FormatearFechaEmision(string fechaRaw)
    {
        if (DateTime.TryParse(fechaRaw, out DateTime fecha))
        {
            return fecha.ToString("dd-MM-yyyy");
        }
        return DateTime.Now.ToString("dd-MM-yyyy");
    }

    private static string FormatearMontoTotal(string montoRaw)
    {
        if (decimal.TryParse(montoRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal monto))
        {
            return monto.ToString("0.00", CultureInfo.InvariantCulture);
        }
        return "0.00";
    }

    private async Task GuardarEnBaseDeDatosAsync(
        string rncEmisor,
        string rncComprador,
        string eNCF,
        string xmlEntrante,
        string xmlFirmado,
        CancellationToken cancellationToken)
    {
        const string query = @"
            INSERT INTO EcfAprobacionComercialLog (RNCEmisor, RNCComprador, eNCF, XmlEntrante, XmlRespuestaFirmado, FechaRegistro)
            VALUES (@RNCEmisor, @RNCComprador, @eNCF, @XmlEntrante, @XmlRespuestaFirmado, GETDATE());";

        using (var connection = new SqlConnection(CadenaConexion))
        {
            await connection.OpenAsync(cancellationToken);
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@RNCEmisor", SqlDbType.VarChar, 20).Value = rncEmisor;
                command.Parameters.Add("@RNCComprador", SqlDbType.VarChar, 20).Value = rncComprador;
                command.Parameters.Add("@eNCF", SqlDbType.VarChar, 20).Value = eNCF;
                command.Parameters.Add("@XmlEntrante", SqlDbType.NVarChar, -1).Value = xmlEntrante;
                command.Parameters.Add("@XmlRespuestaFirmado", SqlDbType.NVarChar, -1).Value = xmlFirmado;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }
}