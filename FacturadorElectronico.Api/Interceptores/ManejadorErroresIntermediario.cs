using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FacturadorElectronico.Api.Excepciones;
using FacturadorElectronico.Aplicacion.Envoltorios;
using FacturadorElectronico.Aplicacion.Excepciones;

namespace FacturadorElectronico.Api.Interceptores
{
    public class ManejadorErroresIntermediario(RequestDelegate siguiente, ILogger<ManejadorErroresIntermediario> logeador)
    {
        private readonly RequestDelegate _siguiente = siguiente;
        private readonly ILogger<ManejadorErroresIntermediario> _logeador = logeador;

        /// <summary>
        /// Middleware para manejar excepciones globalmente en la API.
        /// </summary>
        /// <param name="contexto"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext contexto)
        {
            try
            {
                await _siguiente(contexto);
            }
            catch (Exception error)
            {
                HttpResponse respuesta = contexto.Response;
                respuesta.ContentType = "application/json";

                Respuesta<string> modeloRespuesta = new()
                {
                    OperacionExitosa = false,
                    Mensaje = error.Message
                };

                switch (error)
                {
                    case ExcepcionApi e:
                        respuesta.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    case ExcepcionValidacion e:
                        respuesta.StatusCode = (int)HttpStatusCode.BadRequest;
                        modeloRespuesta.Errores = e.Errores;
                        break;

                    case KeyNotFoundException e:
                        respuesta.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    default:
                        modeloRespuesta.Mensaje = error.Message;
                        modeloRespuesta.Errores = [error.InnerException?.Message ?? string.Empty];
                        respuesta.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                // 🔗 Asocia la excepción al trace y span actual
                Activity? activity = Activity.Current;
                activity?.SetStatus(ActivityStatusCode.Error, error.Message);
                activity?.AddEvent(new ActivityEvent("Handled exception", tags: new ActivityTagsCollection
                {
                    { "exception.type", error.GetType().ToString() },
                    { "exception.message", error.Message },
                    { "exception.stacktrace", error.StackTrace ?? string.Empty },
                    { "exception.source", "ErrorHandlerMiddleware" }
                }));

                if (error is ExcepcionValidacion validationEx) modeloRespuesta.Errores = validationEx.Errores;

                // ✅ Usa ILogger (integrado con OTEL) — Aparecerá en la pestaña Logs del servicio
                _logeador.LogError(error, "❌ Mediador de excepción: {Message}", error.Message);

                string resultado = JsonSerializer.Serialize(modeloRespuesta);
                await respuesta.WriteAsync(resultado);
            }
        }
    }
}
