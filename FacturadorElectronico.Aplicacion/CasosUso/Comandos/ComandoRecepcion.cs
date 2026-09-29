using MediatR;

namespace FacturadorElectronico.Aplicacion.CasosUso.Comandos
{
    public record ComandoRecepcion(
        int Id
    ) : IRequest<String>;
}