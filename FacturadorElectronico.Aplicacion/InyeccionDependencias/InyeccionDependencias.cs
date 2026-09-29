using System.Reflection;
using FacturadorElectronico.Aplicacion.Comportamientos;
using Microsoft.Extensions.DependencyInjection;

namespace FacturadorElectronico.Aplicacion.InyeccionDependencias
{
    public static class InyeccionDependencias
    {
        public static void AgregarCapaAplicacion(this IServiceCollection servicios)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            servicios.AddMediatR(mediatRConfiguracion =>
            {
                mediatRConfiguracion.RegisterServicesFromAssemblies(assembly);
                mediatRConfiguracion.AddOpenBehavior(typeof(ValidacionComportamiento<,>));
            });

        }
    }
}
