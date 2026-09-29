using MediatR;

namespace FacturadorElectronico.Aplicacion.CasosUso.Comandos.Facturacion
{
    public record ComandoRecepcion(
        int Id
    ) : IRequest<String>;
}