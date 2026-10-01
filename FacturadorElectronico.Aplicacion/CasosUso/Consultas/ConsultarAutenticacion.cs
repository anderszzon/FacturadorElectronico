using FacturadorElectronico.Aplicacion.Envoltorios;
using MediatR;
using System;

namespace FacturadorElectronico.Aplicacion.CasosUso.Consultas
{
    public record ConsultaAutenticacionSemilla : IRequest<Respuesta<string>>;
}
