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

        public async Task<string> ValidarCertificadoXmlAsync(string url, string xmlContenido, CancellationToken cancellationToken)
        {
            return await FacturacionElectronicaDGII.ValidarCertificadoXmlAsync(url, xmlContenido, cancellationToken);
        }
        public async Task<string> ProcesarRecepcionEcfXmlAsync(string urlRecepcion, string xmlEcf, string tokenBearer, string nombreArchivoXml = "ecf.xml", CancellationToken cancellationToken = default)
        {
            return await FacturacionElectronicaDGII.EnviarFacturaElectronicaAsync(urlRecepcion, xmlEcf, tokenBearer, nombreArchivoXml, cancellationToken);
        }

        public string FirmarAcuseRecibo(string xmlAcuseSinFirmar, string passCert = "")
        {
            return FacturacionElectronicaDGII.FirmarAcuseRecibo(xmlAcuseSinFirmar, passCert);
        }
    }
}
