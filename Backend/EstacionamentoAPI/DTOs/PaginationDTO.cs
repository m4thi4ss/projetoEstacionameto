namespace EstacionamentoAPI.DTOs
{
    /// <summary>
    /// Parâmetros de paginação para requisições
    /// </summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }

    /// <summary>
    /// Resultado paginado genérico
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Filtros para Veículos
    /// </summary>
    public class VeiculoFiltros : PaginationParams
    {
        public string? Placa { get; set; }
        public int? Tipo { get; set; }
        public string? OrderBy { get; set; } = "placa"; // placa, modelo, tipo, data
        public bool Descending { get; set; } = false;
    }

    /// <summary>
    /// Filtros para Sessões
    /// </summary>
    public class SessaoFiltros : PaginationParams
    {
        public string? Placa { get; set; }
        public bool? SessaoAberta { get; set; } // null = todas, true = abertas, false = fechadas
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? OrderBy { get; set; } = "dataEntrada"; // dataEntrada, dataSaida, valor, placa
        public bool Descending { get; set; } = true; // Padrão: mais recentes primeiro
    }
}
