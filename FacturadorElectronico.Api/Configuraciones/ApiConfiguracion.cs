namespace FacturadorElectronico.Api.Configuraciones
{
    public record ApiConfiguracion(
        string OtelLogsEndpoint,
        string OtelTracesEndpoint,
        string OtelHeaders,
        string NombreServicio,
        string[] OrigenesCORSPermitidos);
}
