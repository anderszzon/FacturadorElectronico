namespace FacturadorElectronico.Aplicacion.Envoltorios
{
    public sealed class RespuestaObjetoPaginado<T>(Paginacion paginacion, T value)
        : Respuesta<T>(value)
    {
        public Paginacion Paginacion { get; set; } = paginacion;

        public static implicit operator RespuestaObjetoPaginado<T>((Paginacion paginacion, T dato) value)
            => new(value.paginacion, value.dato);
    }
}
