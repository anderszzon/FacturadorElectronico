namespace FacturadorElectronico.Aplicacion.Excepciones
{
    public class ExcepcionAplicacion : Exception
    {
        public ExcepcionAplicacion()
        {
        }

        public ExcepcionAplicacion(string? message) : base(message)
        {
        }

        public ExcepcionAplicacion(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
