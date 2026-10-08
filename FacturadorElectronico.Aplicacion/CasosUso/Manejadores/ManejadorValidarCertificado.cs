using FacturadorElectronico.Aplicacion.CasosUso.Comandos;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Puertos;
using MediatR;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace FacturadorElectronico.Aplicacion.CasosUso.Manejadores;

public class ManejadorValidarCertificado(
    IAutenticacion autenticacion) : IRequestHandler<ComandoValidarCertificado, Respuesta<RespuestaTokenDto>>
{
    private readonly IAutenticacion _autenticacion = autenticacion;

    private const string CadenaConexion = "Server=tcp:mtpprod.database.windows.net,1433;Initial Catalog=MTPDatabase;Persist Security Info=False;User ID=MTPadmin;Password=Pass@word1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

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

            string? tokenExtraido = tokenDto.Token;

            await GuardarLogAutenticacionAsync(urlValidacion, xmlContenido, respuestaJson, tokenExtraido, true, null, cancellationToken);

            return Respuesta<RespuestaTokenDto>.Ok(tokenDto);
        }
        catch (Exception ex)
        {
            return Respuesta<RespuestaTokenDto>.Fail($"Error en la validación del certificado: {ex.Message}");
        }
    }

    private async Task GuardarLogAutenticacionAsync(
        string endpointUrl,
        string? payloadEntrante,
        string? xmlRespuesta,
        string? tokenBearer,
        bool esExitoso,
        string? mensajeError,
        CancellationToken cancellationToken)
    {
        const string query = @"
            INSERT INTO EcfAutenticacionLog (EndpointUrl, PayloadEntrante, XmlRespuesta, TokenBearer, EsExitoso, MensajeError, FechaRegistro)
            VALUES (@EndpointUrl, @PayloadEntrante, @XmlRespuesta, @TokenBearer, @EsExitoso, @MensajeError, GETDATE());";

        using (var connection = new SqlConnection(CadenaConexion))
        {
            await connection.OpenAsync(cancellationToken);
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.Add("@EndpointUrl", SqlDbType.NVarChar, 250).Value = endpointUrl;
                command.Parameters.Add("@PayloadEntrante", SqlDbType.NVarChar, -1).Value = (object?)payloadEntrante ?? DBNull.Value;
                command.Parameters.Add("@XmlRespuesta", SqlDbType.NVarChar, -1).Value = (object?)xmlRespuesta ?? DBNull.Value;
                command.Parameters.Add("@TokenBearer", SqlDbType.NVarChar, -1).Value = (object?)tokenBearer ?? DBNull.Value;
                command.Parameters.Add("@EsExitoso", SqlDbType.Bit).Value = esExitoso;
                command.Parameters.Add("@MensajeError", SqlDbType.NVarChar, -1).Value = (object?)mensajeError ?? DBNull.Value;

                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }
}