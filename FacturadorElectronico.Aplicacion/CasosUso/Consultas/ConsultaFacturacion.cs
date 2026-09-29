using FacturadorElectronico.Aplicacion.DTOs;
using FacturadorElectronico.Aplicacion.Envoltorios;
using MediatR;

namespace FacturadorElectronico.Aplicacion.CasosUso.Consultas
{
    public record ConsultaFacturacion(
        int IdCentroServicioOrigen,
        string? Filtro,
        DateTime? Fecha,
        int? IdEstadoBloque,
        int? NumeroPagina,
        int? TamanoPagina,
        bool? Ascendente) : IRequest<RespuestaObjetoPaginado<RespuestaPruebaConsulta>>;

}