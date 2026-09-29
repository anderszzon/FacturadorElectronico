namespace FacturadorElectronico.Aplicacion.DTOs
{
    public record RespuestaPruebaConsulta(
        string? Semilla,
        string? Mensaje,
        string? CodigoRespuesta,
        string? CodigoError,
        string? DescripcionError
    );
}
