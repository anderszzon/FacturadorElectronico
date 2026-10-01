using FacturadorElectronico.Aplicacion.Puertos;
using FacturadorElectronico.Infraestructura.Adaptadores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FacturadorElectronico.Infraestructura.InyeccionDependencias
{
    public static class ServicioExtension
    {

        public static void AgregarCapaInfraestructura(this IServiceCollection servicios, IConfiguration configuration)
        {
            servicios.AgregarComponentesYRepositorios();
        }

        private static void AgregarComponentesYRepositorios(this IServiceCollection servicios)
        {
            servicios.AddScoped<IAutenticacion, Autenticacion>();
        }

    }
}