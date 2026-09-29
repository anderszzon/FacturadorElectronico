using System.Diagnostics;

namespace FacturadorElectronico.Api.Interceptores
{
    public class LogeoContextoIntermediario
    {
        private readonly RequestDelegate _siguiente;
        private readonly ILogger<LogeoContextoIntermediario> _logeador;

        public LogeoContextoIntermediario(RequestDelegate siguiente, ILogger<LogeoContextoIntermediario> registrador)
        {
            _siguiente = siguiente;
            _logeador = registrador;
        }

        /// <summary>
        /// Middleware para registrar el contexto de la solicitud HTTP.
        /// </summary>
        /// <param name="contexto"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext contexto)
        {
            Activity? actividad = Activity.Current;

            Dictionary<string, object?> itemsContexto = new Dictionary<string, object?>
            {
                ["dt.trace_id"] = actividad?.TraceId.ToString(),
                ["dt.span_id"] = actividad?.SpanId.ToString(),
                ["dt.entity.process_group_instance"] = "",
                ["service.name"] = "Plantilla"
            };

            using (_logeador.BeginScope(itemsContexto))
            {
                await _siguiente(contexto);
            }
        }
    }
}
