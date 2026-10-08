using FacturadorElectronico.Aplicacion.CasosUso.Consultas;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FacturadorElectronico.Aplicacion.CasosUso.Manejadores;
public class ManejadorAutenticacion(
    IAutenticacion autenticacion) : IRequestHandler<ConsultaAutenticacionSemilla, Respuesta<string>>
{
    private readonly IAutenticacion _autenticacion = autenticacion;

    private const string CadenaConexion = "Server=tcp:mtpprod.database.windows.net,1433;Initial Catalog=MTPDatabase;Persist Security Info=False;User ID=MTPadmin;Password=Pass@word1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

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

            await GuardarLogAutenticacionAsync(urlSemilla, xmlSemilla, true, null, cancellationToken);

            return Respuesta<string>.Ok(xmlSemilla);
        }
        catch (Exception ex)
        {
            return Respuesta<string>.Fail($"Error en la petición de semilla: {ex.Message}");
        }
    }

    private async Task GuardarLogAutenticacionAsync(
        string endpointUrl,
        string? xmlRespuesta,
        bool esExitoso,
        string? mensajeError,
        CancellationToken cancellationToken)
    {
        const string query = @"
            INSERT INTO EcfAutenticacionLog (EndpointUrl, XmlRespuesta, EsExitoso, MensajeError, FechaRegistro)
            VALUES (@EndpointUrl, @XmlRespuesta, @EsExitoso, @MensajeError, GETDATE());";

        using (var connection = new SqlConnection(CadenaConexion))
        {
            await connection.OpenAsync(cancellationToken);
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@EndpointUrl", SqlDbType.NVarChar, 250).Value = endpointUrl;
                command.Parameters.Add("@XmlRespuesta", SqlDbType.NVarChar, -1).Value = (object?)xmlRespuesta ?? DBNull.Value;
                command.Parameters.Add("@EsExitoso", SqlDbType.Bit).Value = esExitoso;
                command.Parameters.Add("@MensajeError", SqlDbType.NVarChar, -1).Value = (object?)mensajeError ?? DBNull.Value;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }
}

