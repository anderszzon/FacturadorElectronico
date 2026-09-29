using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;

namespace FacturadorElectronico.Api.Interceptores
{
    public class MalaPeticionLogeadorIntermediario
    {
        private readonly RequestDelegate _siguiente;
        private readonly ILogger<MalaPeticionLogeadorIntermediario> _logeador;

        public MalaPeticionLogeadorIntermediario(RequestDelegate siguiente, ILogger<MalaPeticionLogeadorIntermediario> logeador)
        {
            _siguiente = siguiente ?? throw new ArgumentNullException(nameof(siguiente));
            _logeador = logeador ?? throw new ArgumentNullException(nameof(logeador));
        }

        /// <summary>
        /// Middleware para registrar solicitudes HTTP que resulten en un error 400 (Bad Request).
        /// </summary>
        /// <param name="contexto"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext contexto)
        {
            ArgumentNullException.ThrowIfNull(contexto);

            contexto.Request.EnableBuffering();
            string requestBody = await LeerCuerpoSolicitudAsincrono(contexto);

            await using (await BuferDeRespuesta.BeginAsync(contexto))
            {
                await _siguiente(contexto);

                if (contexto.Response.StatusCode == StatusCodes.Status400BadRequest)
                {
                    string responseBody = BuferDeRespuesta.GetBodyAsString(contexto);
                    LoguearError400(contexto, requestBody, responseBody);
                }
            }
        }

        /// <summary>
        /// Método para leer el cuerpo de la solicitud HTTP de forma asíncrona.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<string> LeerCuerpoSolicitudAsincrono(HttpContext context)
        {
            if (context.Request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) != true)
                return string.Empty;

            context.Request.Body.Position = 0;
            using StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            return body;
        }

        /// <summary>
        /// Método para registrar un error 400 en el log.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requestBody"></param>
        /// <param name="responseBody"></param>
        private void LoguearError400(HttpContext context, string requestBody, string responseBody)
        {
            string traceId = Activity.Current?.TraceId.ToString() ?? "sin-traza";
            string spanId = Activity.Current?.SpanId.ToString() ?? "sin-span";
            string serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "Servicio desconocido";
            string processGroupInstance = Environment.GetEnvironmentVariable("PROCESS_GROUP_INSTANCE") ?? "unknown-instance";

            using (_logeador.BeginScope(new Dictionary<string, object?>
            {
                ["trace_id"] = traceId,
                ["span_id"] = spanId,
                ["service.name"] = serviceName,
                ["dt.entity.process_group_instance"] = processGroupInstance
            }))
            {
                _logeador.LogWarning("""
                ⚠️ Solicitud inválida: {Method} {Url} → 400
                👉 Payload recibido: {RequestBody}
                ❌ Respuesta: {ResponseBody}
                """,
                    context.Request.Method,
                    context.Request.GetDisplayUrl(),
                    string.IsNullOrWhiteSpace(requestBody) ? "[Payload vacío]" : requestBody,
                    string.IsNullOrWhiteSpace(responseBody) ? "[Cuerpo vacío]" : responseBody
                );
            }
        }
    }
}
