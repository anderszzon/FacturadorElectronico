using FacturadorElectronico.Api.Configuraciones;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FacturadorElectronico.Api.Extensiones
{
    public static class ConfigurarTelemetriaDynatrace
    {

        public static ILoggingBuilder ConfigurarDynatraceLogs(this ILoggingBuilder logging, ApiConfiguracion config)
        {

            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddFilter<OpenTelemetryLoggerProvider>("*", LogLevel.Information);
            logging.AddOpenTelemetry(logConfig =>
            {
                logConfig.SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                                   .AddService(serviceName: config.NombreServicio!)
                );
                logConfig.IncludeFormattedMessage = true;
                logConfig.IncludeScopes = true;
                logConfig.ParseStateValues = true;
                logConfig.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(config.OtelLogsEndpoint!);
                    options.Headers = $"Authorization=Api-Token {config.OtelHeaders!}";
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
            });

            return logging;
        }

        public static IServiceCollection ConfigureDynatraceTrazas(this IServiceCollection services, ApiConfiguracion config)
        {
            services.AddOpenTelemetry()
                .WithTracing(tp =>
                {
                    tp.SetResourceBuilder(
                          ResourceBuilder.CreateDefault()
                                         .AddService(config.NombreServicio!))
                      .SetSampler(new AlwaysOnSampler())
                      .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                      .AddHttpClientInstrumentation()
                      .AddOtlpExporter(o =>
                      {
                          o.Endpoint = new Uri(config.OtelTracesEndpoint!);
                          o.Headers = $"Authorization=Api-Token {config.OtelHeaders!}";
                          o.Protocol = OtlpExportProtocol.HttpProtobuf;
                      });
                });

            return services;
        }
    }
}
