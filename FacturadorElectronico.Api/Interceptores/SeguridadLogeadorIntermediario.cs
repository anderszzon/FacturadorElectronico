using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;

namespace FacturadorElectronico.Api.Interceptores
{
    public class SeguridadErrorLogeadorIntermediario(RequestDelegate siguiente, ILogger<SeguridadErrorLogeadorIntermediario> logeador)
    {
        private readonly RequestDelegate _siguiente = siguiente ?? throw new ArgumentNullException(nameof(siguiente));
        private readonly ILogger<SeguridadErrorLogeadorIntermediario> _logeador = logeador ?? throw new ArgumentNullException(nameof(logeador));

        /// <summary>
        /// Middleware para registrar errores de seguridad (401 y 403) en las respuestas HTTP.
        /// </summary>
        /// <param name="contexto"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext contexto)
        {
            await using (await BuferDeRespuesta.BeginAsync(contexto))
            {
                await _siguiente(contexto);

                if (contexto.Response.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
                {
                    string responseBody = BuferDeRespuesta.GetBodyAsString(contexto);

                    string? traceId = Activity.Current?.TraceId.ToString() ?? "sin-traza";
                    string? spanId = Activity.Current?.SpanId.ToString() ?? "sin-span";
                    string? serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "Servicio desconocido";
                    string? processGroupInstance = Environment.GetEnvironmentVariable("PROCESS_GROUP_INSTANCE") ?? "unknown-instance";

                    string? user = contexto.User?.Identity?.IsAuthenticated == true
                        ? contexto.User.Identity?.Name ?? "No autorizado"
                        : "No autenticado";

                    string? mensajeRespuesta = string.IsNullOrWhiteSpace(responseBody)
                        ? (contexto.Response.StatusCode == 401
                            ? "[No autorizado - 401: El usuario no está autenticado]"
                            : "[Prohibido - 403: El usuario no tiene permisos suficientes]")
                        : responseBody;

                    using (_logeador.BeginScope(new Dictionary<string, object?>
                    {
                        ["trace_id"] = traceId,
                        ["span_id"] = spanId,
                        ["service.name"] = serviceName,
                        ["dt.entity.process_group_instance"] = processGroupInstance
                    }))
                    {
                        _logeador.LogWarning("""
                                             🔐 Error de seguridad detectado: {StatusCode}
                                             👤 Usuario: {User}
                                             👉 Método: {Method}
                                             👉 URL: {Url}
                                             ❌ Respuesta: {ResponseBody}
                                             """,
                            contexto.Response.StatusCode,
                            user,
                            contexto.Request.Method,
                            contexto.Request.GetDisplayUrl(),
                            mensajeRespuesta
                        );
                    }
                }
            }
        }
    }
}
