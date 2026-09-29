using FacturadorElectronico.Api.Constantes;

namespace FacturadorElectronico.Api.Excepciones
{
    internal class VariableApiNoConfigurada(string nombreVariable)
        : ExcepcionApi(string.Format(MensajesError.VariableNoConfigurada, nombreVariable));

}
