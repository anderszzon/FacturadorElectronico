namespace FacturadorElectronico.Aplicacion.Envoltorios
{
    public sealed class Paginacion
    {
        public Paginacion()
        {
        }

        public Paginacion(int numeroPagina, int tamanoPagina, int totalArticulos)
        {
            NumeroPagina = numeroPagina;
            TamanoPagina = tamanoPagina;
            TotalArticulos = totalArticulos;
            TotalPaginas = totalArticulos < tamanoPagina ? 1 : (int)Math.Ceiling(totalArticulos / (double)tamanoPagina);
        }

        /// <summary>
        /// Página actual (1-based).
        /// </summary>
        public int NumeroPagina { get; set; }

        /// <summary>
        /// Cantidad de elementos por página.
        /// </summary>
        public int TamanoPagina { get; set; }

        /// <summary>
        /// Total de elementos disponibles en la colección.
        /// </summary>
        public int TotalArticulos { get; set; }

        /// <summary>
        /// Total de páginas calculadas según el tamaño de página y total de artículos.
        /// </summary>
        public int TotalPaginas { get; set; }
    }
}
