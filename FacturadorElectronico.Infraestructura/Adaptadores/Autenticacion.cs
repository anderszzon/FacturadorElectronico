using FacturadorElectronico.Aplicacion.Puertos;
using ConexionDGII;

namespace FacturadorElectronico.Infraestructura.Adaptadores
{
    public class Autenticacion : IAutenticacion
    {
        public async Task<string> ObtenerSemillaXmlAsync(string urlSemilla, CancellationToken cancellationToken)
        {
            return await FacturacionElectronicaDGII.ObtenerSemillaXmlAsync(urlSemilla, cancellationToken);
        }
    }
}
