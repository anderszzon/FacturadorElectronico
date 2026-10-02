namespace FacturadorElectronico.Aplicacion.Puertos
{
    public interface IAutenticacion
    {
        Task<string> ObtenerSemillaXmlAsync(string urlSemilla, CancellationToken cancellationToken);
        Task<string> ValidarCertificadoXmlAsync(string urlValidacion, string xmlSemillaFirmada, CancellationToken cancellationToken = default);
        Task<string> ProcesarRecepcionEcfXmlAsync(string urlRecepcion, string xmlEcf, string tokenBearer, string nombreArchivoXml = "ecf.xml", CancellationToken cancellationToken = default);
    }
}
