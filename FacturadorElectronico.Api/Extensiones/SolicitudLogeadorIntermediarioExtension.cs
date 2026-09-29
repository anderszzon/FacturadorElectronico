using FacturadorElectronico.Api.Interceptores;

namespace FacturadorElectronico.Api.Extensiones
{
    public static class SolicitudLogeadorIntermediarioExtension
    {
        /// <summary>
        /// Método de extensión para registrar el middleware de logeo de solicitudes HTTP en la tubería de solicitudes.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UsarBadRequestLogeador(this IApplicationBuilder app)
        {
            return app.UseMiddleware<MalaPeticionLogeadorIntermediario>();
        }

        /// <summary>
        /// Método de extensión para registrar el middleware de logeo de errores de seguridad (401 y 403) en la tubería de solicitudes HTTP.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UsarSeguridadSolicitudLogeador(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SeguridadErrorLogeadorIntermediario>();
        }
    }
}
