using MediatR;

namespace FacturadorElectronico.Aplicacion.CasosUso.Consultas.Facturacion
{
    public record ConsultaFacturacion(
        int Id
    ) : IRequest<String>;

}