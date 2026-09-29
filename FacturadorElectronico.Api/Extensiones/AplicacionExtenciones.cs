using FacturadorElectronico.Api.Interceptores;

namespace FacturadorElectronico.Api.Extensiones
{
    public static class AplicacionExtensiones
    {
        /// <summary>
        /// Método de extensión para registrar el middleware de manejo de errores en la tubería de solicitudes HTTP.
        /// </summary>
        /// <param name="app"></param>
        public static void UsarManejadorIntermediarioErrores(this IApplicationBuilder app)
        {
            app.UseMiddleware<ManejadorErroresIntermediario>();
        }
        /// <summary>
        /// Método de extensión para registrar el middleware de logeo de contexto en la tubería de solicitudes HTTP.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UsarRegistradorContextoIntermediario(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogeoContextoIntermediario>();
        }
    }
}
