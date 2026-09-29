using FacturadorElectronico.Api.Excepciones;

namespace FacturadorElectronico.Api.Configuraciones
{
    public static class InyeccionDependencias
    {
        public static ApiConfiguracion InstanciarConfiguracionApi(this IServiceCollection servicios)
        {
            ApiConfiguracion configuracion = new
            (
                OtelLogsEndpoint: Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT_LOGS")
                ?? throw new VariableApiNoConfigurada("OTEL_EXPORTER_OTLP_ENDPOINT_LOGS"),
                OtelTracesEndpoint: Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT_TRACES")
                ?? throw new VariableApiNoConfigurada("OTEL_EXPORTER_OTLP_ENDPOINT_TRACES"),
                OtelHeaders: Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS")
                ?? throw new VariableApiNoConfigurada("OTEL_EXPORTER_OTLP_HEADERS"),
                NombreServicio: Environment.GetEnvironmentVariable("NOMBRE_SERVICIO")
                ?? throw new VariableApiNoConfigurada("NOMBRE_SERVICIO"),
                OrigenesCORSPermitidos: Environment.GetEnvironmentVariable("ORIGENES_CORS_PERMITIDOS")?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                ?? throw new VariableApiNoConfigurada("ORIGENES_CORS_PERMITIDOS")
            );
            servicios.AddSingleton(configuracion);
            return configuracion;
        }
    }
}
