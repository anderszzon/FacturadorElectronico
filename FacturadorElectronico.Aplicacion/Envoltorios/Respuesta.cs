namespace FacturadorElectronico.Aplicacion.Envoltorios
{
    /// <summary>
    /// Envoltorio estándar de respuesta con soporte de conversión implícita desde T.
    /// </summary>
    public class Respuesta<T>
    {
        public Respuesta()
        {
            OperacionExitosa = true;
            Mensaje = string.Empty;
            Errores = [];
            Resultado = default;
        }

        public Respuesta(T datos, string? mensaje = null)
        {
            OperacionExitosa = true;
            Mensaje = mensaje;
            Errores = [];
            Resultado = datos;
        }

        public Respuesta(string message)
        {
            OperacionExitosa = false;
            Mensaje = message;
            Errores = [];
            Resultado = default;
        }

        /// <summary>
        /// Crea una respuesta OK explícita (azúcar sobre el ctor).
        /// </summary>
        public static Respuesta<T> Ok(T datos, string? mensaje = null) => new(datos, mensaje);

        /// <summary>
        /// Crea una respuesta de error con lista opcional de errores.
        /// </summary>
        public static Respuesta<T> Fail(string mensaje, IEnumerable<string>? errores = null)
            => new(mensaje)
            {
                OperacionExitosa = false,
                Mensaje = mensaje,
                Errores = errores is null ? [] : new(errores),
                Resultado = default
            };

        public bool OperacionExitosa { get; set; }
        public string? Mensaje { get; set; }
        public List<string> Errores { get; set; }
        public T? Resultado { get; set; }

        /// <summary>
        /// Conversión implícita: permite return datos; donde se espera Respuesta<T>.
        /// </summary>
        public static implicit operator Respuesta<T>(T value) => new(value);

        /// <summary>
        /// Azúcar para pattern matching funcional: maneja OK/Fail en una sola llamada.
        /// </summary>
        public TResult Match<TResult>(Func<T?, TResult> onOk, Func<string?, List<string>, TResult> onFail)
            => OperacionExitosa
                ? onOk(Resultado)
                : onFail(Mensaje, Errores);
    }
}
