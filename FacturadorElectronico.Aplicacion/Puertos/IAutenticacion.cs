namespace FacturadorElectronico.Aplicacion.Puertos
{
    public interface IAutenticacion
    {
        Task<string> ObtenerSemillaXmlAsync(string urlSemilla, CancellationToken cancellationToken);
    }
}
